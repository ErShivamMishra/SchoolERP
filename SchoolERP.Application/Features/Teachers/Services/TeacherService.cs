using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Teachers.Interfaces;
using SchoolERP.Application.Features.Teachers.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Teachers.Services;

public sealed class TeacherService(
    ITeacherRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateTeacherRequestDto> createValidator,
    IValidator<UpdateTeacherRequestDto> updateValidator,
    IValidator<AssignTeacherSubjectsRequestDto> assignSubjectsValidator,
    IValidator<AssignTeacherClassesRequestDto> assignClassesValidator,
    IValidator<GetTeachersRequestDto> listValidator) : ITeacherService
{
    public async Task<TeacherDto> CreateAsync(CreateTeacherRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        await EnsureTeacherUniquenessAsync(schoolId, request.EmployeeCode.Trim(), request.Email?.Trim(), null, cancellationToken);

        var teacher = new Teacher
        {
            SchoolId = schoolId,
            EmployeeCode = request.EmployeeCode.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Gender = request.Gender,
            MobileNumber = request.MobileNumber.Trim(),
            Email = request.Email?.Trim(),
            Qualification = request.Qualification?.Trim(),
            ExperienceYears = request.ExperienceYears,
            JoiningDateUtc = request.JoiningDate,
            Address = request.Address.Trim(),
            IsActive = true,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddTeacherAsync(teacher, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetTeacherByIdAsync(teacher.Id, cancellationToken) ?? teacher;
        await auditService.WriteAsync(ModuleCodes.TeacherManagement, "TeacherCreated", nameof(Teacher), teacher.Id.ToString(), "Success", $"Teacher {teacher.EmployeeCode} created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapTeacher(saved);
    }

    public async Task<TeacherDto> UpdateAsync(Guid teacherId, UpdateTeacherRequestDto request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var teacher = await GetManagedTeacherAsync(teacherId, cancellationToken);
        await EnsureTeacherUniquenessAsync(teacher.SchoolId, request.EmployeeCode.Trim(), request.Email?.Trim(), teacher.Id, cancellationToken);

        teacher.EmployeeCode = request.EmployeeCode.Trim();
        teacher.FirstName = request.FirstName.Trim();
        teacher.LastName = request.LastName.Trim();
        teacher.Gender = request.Gender;
        teacher.MobileNumber = request.MobileNumber.Trim();
        teacher.Email = request.Email?.Trim();
        teacher.Qualification = request.Qualification?.Trim();
        teacher.ExperienceYears = request.ExperienceYears;
        teacher.JoiningDateUtc = request.JoiningDate;
        teacher.Address = request.Address.Trim();
        teacher.ModifiedAtUtc = DateTime.UtcNow;
        teacher.ModifiedBy = currentUserContext.UserId?.ToString();

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetTeacherByIdAsync(teacher.Id, cancellationToken) ?? teacher;
        await auditService.WriteAsync(ModuleCodes.TeacherManagement, "TeacherUpdated", nameof(Teacher), teacher.Id.ToString(), "Success", $"Teacher {teacher.EmployeeCode} updated.", teacher.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapTeacher(saved);
    }

    public async Task<TeacherDto> AssignSubjectsAsync(Guid teacherId, AssignTeacherSubjectsRequestDto request, CancellationToken cancellationToken)
    {
        await assignSubjectsValidator.ValidateAndThrowAsync(request, cancellationToken);
        var teacher = await GetManagedTeacherAsync(teacherId, cancellationToken);
        var subjectIds = request.SubjectIds.Distinct().ToArray();
        var subjects = await repository.GetSubjectsByIdsAsync(teacher.SchoolId, subjectIds, cancellationToken);
        if (subjects.Count != subjectIds.Length)
        {
            throw new NotFoundException("One or more subjects were not found.", "subject_not_found");
        }

        await repository.RemoveTeacherSubjectsAsync(teacher.Subjects);
        await repository.AddTeacherSubjectsAsync(subjects.Select(subject => new TeacherSubject
        {
            SchoolId = teacher.SchoolId,
            TeacherId = teacher.Id,
            SubjectId = subject.Id,
            CreatedBy = currentUserContext.UserId?.ToString()
        }), cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetTeacherByIdAsync(teacher.Id, cancellationToken) ?? teacher;
        await auditService.WriteAsync(ModuleCodes.TeacherManagement, "TeacherSubjectsAssigned", nameof(TeacherSubject), teacher.Id.ToString(), "Success", $"Assigned {subjects.Count} subjects to teacher {teacher.EmployeeCode}.", teacher.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapTeacher(saved);
    }

    public async Task<TeacherDto> AssignClassesAsync(Guid teacherId, AssignTeacherClassesRequestDto request, CancellationToken cancellationToken)
    {
        await assignClassesValidator.ValidateAndThrowAsync(request, cancellationToken);
        var teacher = await GetManagedTeacherAsync(teacherId, cancellationToken);

        foreach (var assignment in request.Assignments)
        {
            var classEntity = await repository.GetClassByIdAsync(assignment.ClassId, cancellationToken)
                ?? throw new NotFoundException("Class not found.", "class_not_found");
            var section = await repository.GetSectionByIdAsync(assignment.SectionId, cancellationToken)
                ?? throw new NotFoundException("Section not found.", "section_not_found");
            var session = await repository.GetAcademicSessionByIdAsync(assignment.AcademicSessionId, cancellationToken)
                ?? throw new NotFoundException("Academic session not found.", "academic_session_not_found");

            if (classEntity.TenantId != teacher.SchoolId || section.TenantId != teacher.SchoolId || session.TenantId != teacher.SchoolId || section.ClassId != assignment.ClassId)
            {
                throw new ForbiddenException("Teacher access is limited to the current school.", "cross_tenant_access_forbidden");
            }
        }

        await repository.RemoveTeacherClassAssignmentsAsync(teacher.ClassAssignments);
        await repository.AddTeacherClassAssignmentsAsync(request.Assignments.Select(assignment => new TeacherClassAssignment
        {
            SchoolId = teacher.SchoolId,
            TeacherId = teacher.Id,
            ClassId = assignment.ClassId,
            SectionId = assignment.SectionId,
            AcademicSessionId = assignment.AcademicSessionId,
            CreatedBy = currentUserContext.UserId?.ToString()
        }), cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetTeacherByIdAsync(teacher.Id, cancellationToken) ?? teacher;
        await auditService.WriteAsync(ModuleCodes.TeacherManagement, "TeacherClassesAssigned", nameof(TeacherClassAssignment), teacher.Id.ToString(), "Success", $"Assigned {request.Assignments.Count} class mappings to teacher {teacher.EmployeeCode}.", teacher.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapTeacher(saved);
    }

    public async Task<TeacherDto> GetByIdAsync(Guid teacherId, CancellationToken cancellationToken)
        => MapTeacher(await GetManagedTeacherAsync(teacherId, cancellationToken));

    public async Task<PagedResult<TeacherDto>> GetAllAsync(GetTeachersRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetTeacherPageAsync(schoolId, request.Search, request.IsActive, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<TeacherDto>
        {
            Items = items.Select(MapTeacher).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TeacherDto> DeactivateAsync(Guid teacherId, DeactivateTeacherRequestDto request, CancellationToken cancellationToken)
    {
        var teacher = await GetManagedTeacherAsync(teacherId, cancellationToken);
        teacher.IsActive = false;
        teacher.ModifiedAtUtc = DateTime.UtcNow;
        teacher.ModifiedBy = currentUserContext.UserId?.ToString();

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetTeacherByIdAsync(teacher.Id, cancellationToken) ?? teacher;
        await auditService.WriteAsync(ModuleCodes.TeacherManagement, "TeacherDeactivated", nameof(Teacher), teacher.Id.ToString(), "Success", $"Teacher {teacher.EmployeeCode} deactivated.", teacher.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapTeacher(saved);
    }

    private async Task<Teacher> GetManagedTeacherAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var teacher = await repository.GetTeacherByIdAsync(teacherId, cancellationToken)
            ?? throw new NotFoundException("Teacher not found.", "teacher_not_found");
        EnsureSchoolAccess(teacher.SchoolId);
        return teacher;
    }

    private async Task EnsureTeacherUniquenessAsync(Guid schoolId, string employeeCode, string? email, Guid? excludeTeacherId, CancellationToken cancellationToken)
    {
        if (await repository.EmployeeCodeExistsAsync(schoolId, employeeCode, excludeTeacherId, cancellationToken))
        {
            throw new ConflictException("Employee code must be unique within the school.", "teacher_employee_code_exists");
        }

        if (!string.IsNullOrWhiteSpace(email) && await repository.TeacherEmailExistsAsync(schoolId, email, excludeTeacherId, cancellationToken))
        {
            throw new ConflictException("Email already exists within the school.", "teacher_email_exists");
        }
    }

    private async Task<Guid> ResolveSchoolIdAsync(Guid? requestedSchoolId, CancellationToken cancellationToken)
    {
        Guid schoolId;
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            schoolId = requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
        }
        else
        {
            if (!currentUserContext.SchoolId.HasValue)
            {
                throw new ForbiddenException("School context is required for this request.", "school_context_required");
            }

            if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
            {
                throw new ForbiddenException("Teacher access is limited to the current school.", "cross_tenant_access_forbidden");
            }

            schoolId = currentUserContext.SchoolId.Value;
        }

        _ = await repository.GetSchoolByIdAsync(schoolId, cancellationToken)
            ?? throw new NotFoundException("School not found.", "school_not_found");
        return schoolId;
    }

    private Guid ResolveSchoolIdForRead(Guid? requestedSchoolId)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            return requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }

        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Teacher access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Teacher access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static TeacherDto MapTeacher(Teacher teacher) => new()
    {
        Id = teacher.Id,
        SchoolId = teacher.SchoolId,
        EmployeeCode = teacher.EmployeeCode,
        FirstName = teacher.FirstName,
        LastName = teacher.LastName,
        Gender = teacher.Gender,
        MobileNumber = teacher.MobileNumber,
        Email = teacher.Email,
        Qualification = teacher.Qualification,
        ExperienceYears = teacher.ExperienceYears,
        JoiningDate = teacher.JoiningDateUtc,
        Address = teacher.Address,
        IsActive = teacher.IsActive,
        Subjects = teacher.Subjects.Select(x => new TeacherSubjectDto
        {
            SubjectId = x.SubjectId,
            SubjectName = x.Subject?.Name ?? string.Empty,
            SubjectCode = x.Subject?.Code ?? string.Empty
        }).ToArray(),
        ClassAssignments = teacher.ClassAssignments.Select(x => new TeacherClassAssignmentDto
        {
            ClassId = x.ClassId,
            ClassName = x.Class?.Name ?? string.Empty,
            SectionId = x.SectionId,
            SectionName = x.Section?.Name ?? string.Empty,
            AcademicSessionId = x.AcademicSessionId,
            AcademicSessionName = x.AcademicSession?.Name ?? string.Empty
        }).ToArray(),
        CreatedAt = teacher.CreatedAtUtc
    };
}
