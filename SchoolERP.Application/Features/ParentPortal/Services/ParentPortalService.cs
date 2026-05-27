using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.ParentPortal.Interfaces;
using SchoolERP.Application.Features.ParentPortal.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.ParentPortal.Services;

public sealed class ParentPortalService(
    IParentPortalRepository repository,
    IPasswordHasher passwordHasher,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateParentRequestDto> createValidator,
    IValidator<LinkParentStudentRequestDto> linkValidator) : IParentPortalService
{
    public async Task<CreateParentResultDto> CreateParentAsync(CreateParentRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveManagedSchoolIdAsync(request.SchoolId, cancellationToken);
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        if (await repository.ParentEmailExistsAsync(schoolId, normalizedEmail, cancellationToken))
        {
            throw new ConflictException("A parent with this email already exists in the current school.", "parent_email_exists");
        }

        var parentRole = await EnsureParentRoleAsync(schoolId, cancellationToken);
        var temporaryPassword = $"Parent@{Guid.NewGuid():N}"[..18];
        var user = new User
        {
            TenantId = schoolId,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PhoneNumber = request.PhoneNumber.Trim(),
            PasswordHash = passwordHasher.HashPassword(temporaryPassword),
            RoleId = parentRole.Id,
            IsActive = true,
            IsPlatformUser = false,
            RequiresPasswordReset = true,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        var parent = new Parent
        {
            SchoolId = schoolId,
            User = user,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AlternatePhoneNumber = request.AlternatePhoneNumber?.Trim(),
            Address = request.Address?.Trim(),
            Occupation = request.Occupation?.Trim(),
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddUserAsync(user, cancellationToken);
        await repository.AddParentAsync(parent, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(ModuleCodes.ParentPortalManagement, "ParentCreated", nameof(Parent), parent.Id.ToString(), "Success", "Parent account created.", schoolId, currentUserContext.UserId, cancellationToken);

        return new CreateParentResultDto
        {
            Parent = MapParent(parent),
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<ParentLinkedStudentDto> LinkStudentAsync(Guid parentId, LinkParentStudentRequestDto request, CancellationToken cancellationToken)
    {
        await linkValidator.ValidateAndThrowAsync(request, cancellationToken);
        var parent = await repository.GetParentByIdAsync(parentId, cancellationToken)
            ?? throw new NotFoundException("Parent not found.", "parent_not_found");
        EnsureTenantAccess(parent.SchoolId);

        var student = await repository.GetStudentByIdAsync(parent.SchoolId, request.StudentId, cancellationToken)
            ?? throw new NotFoundException("Student not found.", "student_not_found");

        var existing = await repository.GetParentStudentRelationAsync(parent.Id, student.Id, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("This parent is already linked to the selected student.", "parent_student_relation_exists");
        }

        var relation = new ParentStudentRelation
        {
            SchoolId = parent.SchoolId,
            ParentId = parent.Id,
            StudentId = student.Id,
            RelationshipType = request.RelationshipType.Trim(),
            IsPrimaryContact = request.IsPrimaryContact,
            CanViewAttendance = request.CanViewAttendance,
            CanViewFees = request.CanViewFees,
            CanViewResults = request.CanViewResults,
            CanViewHomework = request.CanViewHomework,
            CanViewNotices = request.CanViewNotices,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddParentStudentRelationAsync(relation, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.ParentPortalManagement, "ParentStudentLinked", nameof(ParentStudentRelation), relation.Id.ToString(), "Success", "Parent linked to student.", parent.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapRelation(student, relation);
    }

    public async Task<IReadOnlyCollection<ParentLinkedStudentDto>> GetMyStudentsAsync(CancellationToken cancellationToken)
    {
        var parent = await GetCurrentParentAsync(cancellationToken);
        var relations = await repository.GetStudentRelationsAsync(parent.Id, cancellationToken);
        return relations.Select(x => MapRelation(x.Student!, x)).ToArray();
    }

    public async Task<ParentAttendanceSummaryDto> GetMyStudentAttendanceAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var relation = await GetAuthorizedRelationAsync(studentId, cancellationToken);
        if (!relation.CanViewAttendance)
        {
            throw new ForbiddenException("Attendance access is not enabled for this parent-student relation.", "attendance_access_denied");
        }

        var summary = await repository.GetAttendanceSummaryAsync(relation.SchoolId, studentId, cancellationToken);
        return new ParentAttendanceSummaryDto
        {
            StudentId = studentId,
            PresentDays = summary?.PresentCount ?? 0,
            AbsentDays = summary?.AbsentCount ?? 0,
            LateDays = summary?.LateCount ?? 0,
            AttendancePercentage = CalculateAttendancePercentage(summary)
        };
    }

    public async Task<ParentFeeStatusDto> GetMyStudentFeeStatusAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var relation = await GetAuthorizedRelationAsync(studentId, cancellationToken);
        if (!relation.CanViewFees)
        {
            throw new ForbiddenException("Fee access is not enabled for this parent-student relation.", "fee_access_denied");
        }

        var invoices = await repository.GetInvoicesAsync(relation.SchoolId, studentId, cancellationToken);
        return new ParentFeeStatusDto
        {
            StudentId = studentId,
            TotalInvoices = invoices.Count,
            TotalAmount = invoices.Sum(x => x.TotalAmount),
            PaidAmount = invoices.Sum(x => x.PaidAmount),
            PendingAmount = invoices.Sum(x => x.PendingAmount)
        };
    }

    public async Task<IReadOnlyCollection<ParentResultSummaryDto>> GetMyStudentResultsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var relation = await GetAuthorizedRelationAsync(studentId, cancellationToken);
        if (!relation.CanViewResults)
        {
            throw new ForbiddenException("Result access is not enabled for this parent-student relation.", "result_access_denied");
        }

        var results = await repository.GetExamResultsAsync(relation.SchoolId, studentId, cancellationToken);
        return results
            .GroupBy(x => x.ExamId)
            .Select(group =>
            {
                var totalMarks = group.Sum(x => x.ExamSubject?.MaxMarks ?? 0);
                var obtainedMarks = group.Sum(x => x.ObtainedMarks);
                var first = group.First();
                return new ParentResultSummaryDto
                {
                    ExamId = group.Key,
                    ExamTitle = first.Exam?.Title ?? string.Empty,
                    ObtainedMarks = obtainedMarks,
                    TotalMarks = totalMarks,
                    Percentage = totalMarks == 0 ? 0 : decimal.Round((obtainedMarks / totalMarks) * 100m, 2),
                    IsPublished = first.Exam?.IsPublished ?? false
                };
            })
            .OrderByDescending(x => x.ExamTitle)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<ParentHomeworkDto>> GetMyStudentHomeworkAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var relation = await GetAuthorizedRelationAsync(studentId, cancellationToken);
        if (!relation.CanViewHomework)
        {
            throw new ForbiddenException("Homework access is not enabled for this parent-student relation.", "homework_access_denied");
        }

        var student = relation.Student ?? throw new NotFoundException("Student not found.", "student_not_found");
        var homeworkItems = await repository.GetHomeworkAsync(relation.SchoolId, student.ClassId, student.SectionId, cancellationToken);
        return homeworkItems.Select(x => new ParentHomeworkDto
        {
            HomeworkId = x.Id,
            Title = x.Title,
            Description = x.Instructions,
            AssignedDateUtc = x.CreatedAtUtc,
            DueDateUtc = x.DueDateUtc,
            SubjectName = x.Subject?.Name ?? string.Empty
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<ParentNoticeDto>> GetMyNoticesAsync(CancellationToken cancellationToken)
    {
        var parent = await GetCurrentParentAsync(cancellationToken);
        var relations = await repository.GetStudentRelationsAsync(parent.Id, cancellationToken);
        var resolved = new List<NoticeBoardItem>();
        foreach (var relation in relations.Where(x => x.CanViewNotices && x.Student is not null))
        {
            resolved.AddRange(await repository.GetPublishedNoticesAsync(relation.SchoolId, relation.Student!.ClassId, relation.Student.SectionId, cancellationToken));
        }

        return resolved
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .OrderByDescending(x => x.PublishedAtUtc ?? x.CreatedAtUtc)
            .Select(x => new ParentNoticeDto
            {
                NoticeId = x.Id,
                Title = x.Title,
                Content = x.Content,
                PublishedAtUtc = x.PublishedAtUtc,
                AudienceType = x.AudienceType.ToString(),
                NoticeType = x.NoticeType.ToString()
            })
            .ToArray();
    }

    private async Task<Role> EnsureParentRoleAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var role = await repository.GetRoleByCodeAsync(schoolId, RoleNames.Parent, cancellationToken);
        if (role is null)
        {
            role = new Role
            {
                TenantId = schoolId,
                Name = RoleNames.Parent,
                Code = RoleNames.Parent,
                IsSystemRole = true
            };
            await repository.AddRoleAsync(role, cancellationToken);
        }

        var permission = await repository.GetPermissionByCodeAsync($"{ModuleCodes.ParentPortalManagement}.{PermissionActions.View}", cancellationToken)
            ?? throw new NotFoundException("Parent portal permission is not configured.", "parent_portal_permission_missing");

        if (!await repository.HasRolePermissionAsync(role.Id, permission.Id, cancellationToken))
        {
            await repository.AddRolePermissionAsync(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            }, cancellationToken);
        }

        return role;
    }

    private async Task<Parent> GetCurrentParentAsync(CancellationToken cancellationToken)
    {
        if (!currentUserContext.UserId.HasValue)
        {
            throw new UnauthorizedException("Authentication is required.", "authentication_required");
        }

        var parent = await repository.GetParentByUserIdAsync(currentUserContext.UserId.Value, cancellationToken)
            ?? throw new ForbiddenException("Parent profile is not linked to the current user.", "parent_profile_not_found");

        EnsureTenantAccess(parent.SchoolId);
        return parent;
    }

    private async Task<ParentStudentRelation> GetAuthorizedRelationAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var parent = await GetCurrentParentAsync(cancellationToken);
        var relation = await repository.GetParentStudentRelationAsync(parent.Id, studentId, cancellationToken)
            ?? throw new ForbiddenException("Parent access is limited to linked students only.", "parent_student_access_denied");

        return relation;
    }

    private async Task<Guid> ResolveManagedSchoolIdAsync(Guid? requestedSchoolId, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            var schoolId = requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
            _ = await repository.GetSchoolByIdAsync(schoolId, cancellationToken) ?? throw new NotFoundException("School not found.", "school_not_found");
            return schoolId;
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }

        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Parent access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureTenantAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Parent access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static ParentDto MapParent(Parent parent) => new()
    {
        Id = parent.Id,
        SchoolId = parent.SchoolId,
        UserId = parent.UserId,
        FullName = parent.FullName,
        Email = parent.Email,
        PhoneNumber = parent.PhoneNumber,
        AlternatePhoneNumber = parent.AlternatePhoneNumber,
        Address = parent.Address,
        Occupation = parent.Occupation,
        IsActive = parent.IsActive
    };

    private static ParentLinkedStudentDto MapRelation(Student student, ParentStudentRelation relation) => new()
    {
        StudentId = student.Id,
        StudentName = $"{student.FirstName} {student.LastName}".Trim(),
        RollNumber = student.RollNumber,
        ClassId = student.ClassId,
        ClassName = student.Class?.Name ?? string.Empty,
        SectionId = student.SectionId,
        SectionName = student.Section?.Name ?? string.Empty,
        RelationshipType = relation.RelationshipType,
        IsPrimaryContact = relation.IsPrimaryContact
    };

    private static decimal CalculateAttendancePercentage(StudentAttendanceSummary? summary)
    {
        if (summary is null)
        {
            return 0;
        }

        var total = summary.PresentCount + summary.AbsentCount + summary.LateCount + summary.LeaveCount + summary.HalfDayCount;
        return total == 0 ? 0 : decimal.Round(((summary.PresentCount + summary.LateCount + summary.HalfDayCount) / (decimal)total) * 100m, 2);
    }
}
