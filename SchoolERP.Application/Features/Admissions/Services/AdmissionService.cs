using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Admissions.Interfaces;
using SchoolERP.Application.Features.Admissions.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Admissions.Services;

public sealed class AdmissionService(
    IAdmissionRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateAcademicSessionRequestDto> createSessionValidator,
    IValidator<CreateClassRequestDto> createClassValidator,
    IValidator<CreateSectionRequestDto> createSectionValidator,
    IValidator<CreateAdmissionRequestDto> createValidator,
    IValidator<UpdateAdmissionRequestDto> updateValidator,
    IValidator<GetAdmissionsRequestDto> listValidator) : IAdmissionService
{
    public async Task<AcademicSessionDto> CreateAcademicSessionAsync(CreateAcademicSessionRequestDto request, CancellationToken cancellationToken)
    {
        await createSessionValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);

        if (await repository.AcademicSessionNameExistsAsync(schoolId, request.Name.Trim(), cancellationToken))
        {
            throw new ConflictException("An academic session with this name already exists in the school.", "academic_session_exists");
        }

        var entity = new AcademicSession
        {
            TenantId = schoolId,
            Name = request.Name.Trim(),
            StartDateUtc = request.StartDate,
            EndDateUtc = request.EndDate,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddAcademicSessionAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.AdmissionManagement, "AcademicSessionCreated", nameof(AcademicSession), entity.Id.ToString(), "Success", $"Academic session {entity.Name} created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapSession(entity);
    }

    public async Task<IReadOnlyCollection<AcademicSessionDto>> GetAcademicSessionsAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        return (await repository.GetAcademicSessionsAsync(resolvedSchoolId, cancellationToken)).Select(MapSession).ToArray();
    }

    public async Task<ClassDto> CreateClassAsync(CreateClassRequestDto request, CancellationToken cancellationToken)
    {
        await createClassValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);

        if (await repository.ClassNameExistsAsync(schoolId, request.Name.Trim(), cancellationToken))
        {
            throw new ConflictException("A class with this name already exists in the school.", "class_exists");
        }

        var entity = new Class
        {
            TenantId = schoolId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddClassAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.AdmissionManagement, "ClassCreated", nameof(Class), entity.Id.ToString(), "Success", $"Class {entity.Name} created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapClass(entity);
    }

    public async Task<IReadOnlyCollection<ClassDto>> GetClassesAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        return (await repository.GetClassesAsync(resolvedSchoolId, cancellationToken)).Select(MapClass).ToArray();
    }

    public async Task<SectionDto> CreateSectionAsync(CreateSectionRequestDto request, CancellationToken cancellationToken)
    {
        await createSectionValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var classEntity = await repository.GetClassByIdAsync(request.ClassId, cancellationToken)
            ?? throw new NotFoundException("Class not found.", "class_not_found");

        EnsureSchoolOwnership(schoolId, classEntity.TenantId, "Class access is limited to the current school.");

        if (await repository.SectionNameExistsAsync(request.ClassId, request.Name.Trim(), cancellationToken))
        {
            throw new ConflictException("A section with this name already exists for the class.", "section_exists");
        }

        var entity = new Section
        {
            TenantId = schoolId,
            ClassId = request.ClassId,
            Name = request.Name.Trim(),
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddSectionAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.AdmissionManagement, "SectionCreated", nameof(Section), entity.Id.ToString(), "Success", $"Section {entity.Name} created.", schoolId, currentUserContext.UserId, cancellationToken);
        return new SectionDto
        {
            Id = entity.Id,
            SchoolId = entity.TenantId,
            ClassId = entity.ClassId,
            ClassName = classEntity.Name,
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAtUtc
        };
    }

    public async Task<IReadOnlyCollection<SectionDto>> GetSectionsAsync(Guid? schoolId, Guid? classId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        return (await repository.GetSectionsAsync(resolvedSchoolId, classId, cancellationToken))
            .Select(x => new SectionDto
            {
                Id = x.Id,
                SchoolId = x.TenantId,
                ClassId = x.ClassId,
                ClassName = x.Class?.Name ?? string.Empty,
                Name = x.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAtUtc
            })
            .ToArray();
    }

    public async Task<AdmissionDto> CreateAsync(CreateAdmissionRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        await EnsureAdmissionDependenciesAsync(schoolId, request.AppliedClassId, request.AcademicSessionId, cancellationToken);
        await EnsureAdmissionUniquenessAsync(schoolId, request.AdmissionNumber.Trim(), request.MobileNumber.Trim(), request.Email?.Trim(), null, cancellationToken);

        var entity = new Admission
        {
            SchoolId = schoolId,
            AdmissionNumber = request.AdmissionNumber.Trim(),
            StudentFirstName = request.StudentFirstName.Trim(),
            StudentLastName = request.StudentLastName.Trim(),
            Gender = request.Gender,
            DateOfBirthUtc = request.DateOfBirth,
            MobileNumber = request.MobileNumber.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address.Trim(),
            PreviousSchool = request.PreviousSchool?.Trim(),
            GuardianName = request.GuardianName.Trim(),
            GuardianPhone = request.GuardianPhone.Trim(),
            GuardianRelation = request.GuardianRelation.Trim(),
            AppliedClassId = request.AppliedClassId,
            AcademicSessionId = request.AcademicSessionId,
            AdmissionDateUtc = request.AdmissionDate,
            Status = AdmissionStatus.Pending,
            Remarks = request.Remarks?.Trim(),
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        entity.GuardianDetails = new GuardianDetails
        {
            SchoolId = schoolId,
            GuardianName = entity.GuardianName,
            GuardianPhone = entity.GuardianPhone,
            GuardianRelation = entity.GuardianRelation,
            Email = entity.Email,
            Address = entity.Address,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddAdmissionAsync(entity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var saved = await repository.GetAdmissionByIdAsync(entity.Id, cancellationToken)
            ?? throw new NotFoundException("Admission not found after creation.", "admission_not_found");

        await auditService.WriteAsync(ModuleCodes.AdmissionManagement, "AdmissionCreated", nameof(Admission), saved.Id.ToString(), "Success", $"Admission {saved.AdmissionNumber} created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapAdmission(saved);
    }

    public async Task<AdmissionDto> UpdateAsync(Guid admissionId, UpdateAdmissionRequestDto request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var entity = await GetManagedAdmissionAsync(admissionId, cancellationToken);

        if (entity.Status == AdmissionStatus.Approved && entity.Student is not null)
        {
            throw new BadRequestException("Admissions already converted to students cannot be updated.", "admission_already_converted");
        }

        await EnsureAdmissionDependenciesAsync(entity.SchoolId, request.AppliedClassId, request.AcademicSessionId, cancellationToken);
        await EnsureAdmissionUniquenessAsync(entity.SchoolId, request.AdmissionNumber.Trim(), request.MobileNumber.Trim(), request.Email?.Trim(), entity.Id, cancellationToken);

        entity.AdmissionNumber = request.AdmissionNumber.Trim();
        entity.StudentFirstName = request.StudentFirstName.Trim();
        entity.StudentLastName = request.StudentLastName.Trim();
        entity.Gender = request.Gender;
        entity.DateOfBirthUtc = request.DateOfBirth;
        entity.MobileNumber = request.MobileNumber.Trim();
        entity.Email = request.Email?.Trim();
        entity.Address = request.Address.Trim();
        entity.PreviousSchool = request.PreviousSchool?.Trim();
        entity.GuardianName = request.GuardianName.Trim();
        entity.GuardianPhone = request.GuardianPhone.Trim();
        entity.GuardianRelation = request.GuardianRelation.Trim();
        entity.AppliedClassId = request.AppliedClassId;
        entity.AcademicSessionId = request.AcademicSessionId;
        entity.AdmissionDateUtc = request.AdmissionDate;
        entity.Remarks = request.Remarks?.Trim();
        entity.ModifiedAtUtc = DateTime.UtcNow;
        entity.ModifiedBy = currentUserContext.UserId?.ToString();

        if (entity.GuardianDetails is not null)
        {
            entity.GuardianDetails.GuardianName = entity.GuardianName;
            entity.GuardianDetails.GuardianPhone = entity.GuardianPhone;
            entity.GuardianDetails.GuardianRelation = entity.GuardianRelation;
            entity.GuardianDetails.Email = entity.Email;
            entity.GuardianDetails.Address = entity.Address;
            entity.GuardianDetails.ModifiedAtUtc = DateTime.UtcNow;
            entity.GuardianDetails.ModifiedBy = currentUserContext.UserId?.ToString();
        }

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetAdmissionByIdAsync(entity.Id, cancellationToken) ?? entity;
        await auditService.WriteAsync(ModuleCodes.AdmissionManagement, "AdmissionUpdated", nameof(Admission), entity.Id.ToString(), "Success", $"Admission {entity.AdmissionNumber} updated.", entity.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapAdmission(saved);
    }

    public Task<AdmissionDto> ApproveAsync(Guid admissionId, ChangeAdmissionStatusRequestDto request, CancellationToken cancellationToken)
        => ChangeStatusAsync(admissionId, AdmissionStatus.Approved, request.Remarks, cancellationToken);

    public Task<AdmissionDto> RejectAsync(Guid admissionId, ChangeAdmissionStatusRequestDto request, CancellationToken cancellationToken)
        => ChangeStatusAsync(admissionId, AdmissionStatus.Rejected, request.Remarks, cancellationToken);

    public async Task<AdmissionDto> GetByIdAsync(Guid admissionId, CancellationToken cancellationToken)
        => MapAdmission(await GetManagedAdmissionAsync(admissionId, cancellationToken));

    public async Task<PagedResult<AdmissionDto>> GetAllAsync(GetAdmissionsRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetAdmissionPageAsync(schoolId, request.Search, request.Status, request.AppliedClassId, request.AcademicSessionId, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<AdmissionDto>
        {
            Items = items.Select(MapAdmission).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private async Task<AdmissionDto> ChangeStatusAsync(Guid admissionId, AdmissionStatus status, string? remarks, CancellationToken cancellationToken)
    {
        var entity = await GetManagedAdmissionAsync(admissionId, cancellationToken);
        entity.Status = status;
        entity.Remarks = remarks?.Trim();
        entity.ModifiedAtUtc = DateTime.UtcNow;
        entity.ModifiedBy = currentUserContext.UserId?.ToString();
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.AdmissionManagement, status == AdmissionStatus.Approved ? "AdmissionApproved" : "AdmissionRejected", nameof(Admission), entity.Id.ToString(), "Success", $"Admission {entity.AdmissionNumber} marked as {status}.", entity.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapAdmission(await repository.GetAdmissionByIdAsync(entity.Id, cancellationToken) ?? entity);
    }

    private async Task<Admission> GetManagedAdmissionAsync(Guid admissionId, CancellationToken cancellationToken)
    {
        var entity = await repository.GetAdmissionByIdAsync(admissionId, cancellationToken)
            ?? throw new NotFoundException("Admission not found.", "admission_not_found");

        EnsureSchoolAccess(entity.SchoolId);
        return entity;
    }

    private async Task EnsureAdmissionDependenciesAsync(Guid schoolId, Guid classId, Guid academicSessionId, CancellationToken cancellationToken)
    {
        var classEntity = await repository.GetClassByIdAsync(classId, cancellationToken)
            ?? throw new NotFoundException("Class not found.", "class_not_found");
        var session = await repository.GetAcademicSessionByIdAsync(academicSessionId, cancellationToken)
            ?? throw new NotFoundException("Academic session not found.", "academic_session_not_found");

        EnsureSchoolOwnership(schoolId, classEntity.TenantId, "Class access is limited to the current school.");
        EnsureSchoolOwnership(schoolId, session.TenantId, "Academic session access is limited to the current school.");
    }

    private async Task EnsureAdmissionUniquenessAsync(Guid schoolId, string admissionNumber, string mobileNumber, string? email, Guid? excludeAdmissionId, CancellationToken cancellationToken)
    {
        if (await repository.AdmissionNumberExistsAsync(schoolId, admissionNumber, excludeAdmissionId, cancellationToken))
        {
            throw new ConflictException("Admission number must be unique within the school.", "admission_number_exists");
        }

        if (await repository.MobileExistsAsync(schoolId, mobileNumber, excludeAdmissionId, cancellationToken))
        {
            throw new ConflictException("Mobile number already exists within the school.", "admission_mobile_exists");
        }

        if (!string.IsNullOrWhiteSpace(email) && await repository.EmailExistsAsync(schoolId, email, excludeAdmissionId, cancellationToken))
        {
            throw new ConflictException("Email already exists within the school.", "admission_email_exists");
        }
    }

    private async Task<Guid> ResolveSchoolIdAsync(Guid? requestedSchoolId, CancellationToken cancellationToken)
    {
        Guid schoolId;
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            if (!requestedSchoolId.HasValue)
            {
                throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
            }

            schoolId = requestedSchoolId.Value;
        }
        else
        {
            if (!currentUserContext.SchoolId.HasValue)
            {
                throw new ForbiddenException("School context is required for this request.", "school_context_required");
            }

            if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
            {
                throw new ForbiddenException("Admission access is limited to the current school.", "cross_tenant_access_forbidden");
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
            throw new ForbiddenException("Admission access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Admission access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static void EnsureSchoolOwnership(Guid expectedSchoolId, Guid actualSchoolId, string message)
    {
        if (expectedSchoolId != actualSchoolId)
        {
            throw new ForbiddenException(message, "cross_tenant_access_forbidden");
        }
    }

    private static AcademicSessionDto MapSession(AcademicSession entity) => new()
    {
        Id = entity.Id,
        SchoolId = entity.TenantId,
        Name = entity.Name,
        StartDate = entity.StartDateUtc,
        EndDate = entity.EndDateUtc,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAtUtc
    };

    private static ClassDto MapClass(Class entity) => new()
    {
        Id = entity.Id,
        SchoolId = entity.TenantId,
        Name = entity.Name,
        Description = entity.Description,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAtUtc
    };

    private static AdmissionDto MapAdmission(Admission entity) => new()
    {
        Id = entity.Id,
        SchoolId = entity.SchoolId,
        AdmissionNumber = entity.AdmissionNumber,
        StudentFirstName = entity.StudentFirstName,
        StudentLastName = entity.StudentLastName,
        Gender = entity.Gender,
        DateOfBirth = entity.DateOfBirthUtc,
        MobileNumber = entity.MobileNumber,
        Email = entity.Email,
        Address = entity.Address,
        PreviousSchool = entity.PreviousSchool,
        GuardianName = entity.GuardianName,
        GuardianPhone = entity.GuardianPhone,
        GuardianRelation = entity.GuardianRelation,
        AppliedClassId = entity.AppliedClassId,
        AppliedClassName = entity.AppliedClass?.Name ?? string.Empty,
        AcademicSessionId = entity.AcademicSessionId,
        AcademicSessionName = entity.AcademicSession?.Name ?? string.Empty,
        AdmissionDate = entity.AdmissionDateUtc,
        Status = entity.Status,
        Remarks = entity.Remarks,
        IsConvertedToStudent = entity.Student is not null,
        CreatedAt = entity.CreatedAtUtc
    };
}
