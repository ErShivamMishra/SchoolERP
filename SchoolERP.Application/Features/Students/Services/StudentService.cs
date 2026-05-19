using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Students.Interfaces;
using SchoolERP.Application.Features.Students.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Students.Services;

public sealed class StudentService(
    IStudentRepository repository,
    IFileStorageService fileStorageService,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateStudentRequestDto> createValidator,
    IValidator<UpdateStudentRequestDto> updateValidator,
    IValidator<ConvertAdmissionToStudentRequestDto> convertValidator,
    IValidator<PromoteStudentRequestDto> promoteValidator,
    IValidator<TransferStudentRequestDto> transferValidator,
    IValidator<UploadStudentDocumentRequestDto> documentValidator,
    IValidator<GetStudentsRequestDto> listValidator) : IStudentService
{
    private static readonly string[] AllowedDocumentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    public async Task<StudentDto> ConvertAdmissionAsync(ConvertAdmissionToStudentRequestDto request, CancellationToken cancellationToken)
    {
        await convertValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var admission = await repository.GetAdmissionByIdAsync(request.AdmissionId, cancellationToken)
            ?? throw new NotFoundException("Admission not found.", "admission_not_found");

        EnsureSchoolAccess(admission.SchoolId);

        if (admission.Status != AdmissionStatus.Approved)
        {
            throw new BadRequestException("Only approved admissions can be converted into students.", "admission_not_approved");
        }

        if (admission.Student is not null)
        {
            throw new ConflictException("This admission has already been converted into a student.", "admission_already_converted");
        }

        await EnsureStudentDependenciesAsync(schoolId, request.ClassId, request.SectionId, request.AcademicSessionId, cancellationToken);
        await EnsureStudentUniquenessAsync(schoolId, request.ClassId, request.SectionId, request.AcademicSessionId, request.RollNumber.Trim(), admission.Email?.Trim(), admission.MobileNumber.Trim(), null, cancellationToken);

        var student = new Student
        {
            SchoolId = schoolId,
            AdmissionId = admission.Id,
            RollNumber = request.RollNumber.Trim(),
            FirstName = admission.StudentFirstName,
            LastName = admission.StudentLastName,
            Gender = admission.Gender,
            DateOfBirthUtc = admission.DateOfBirthUtc,
            MobileNumber = admission.MobileNumber,
            Email = admission.Email,
            Address = admission.Address,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            AcademicSessionId = request.AcademicSessionId,
            AdmissionDateUtc = admission.AdmissionDateUtc,
            BloodGroup = request.BloodGroup?.Trim(),
            Religion = request.Religion?.Trim(),
            Category = request.Category?.Trim(),
            AadhaarNumber = request.AadhaarNumber?.Trim(),
            IsActive = true,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        var academicInfo = new StudentAcademicInfo
        {
            SchoolId = schoolId,
            StudentId = student.Id,
            PreviousSchool = admission.PreviousSchool,
            Remarks = admission.Remarks,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddStudentAsync(student, cancellationToken);
        await repository.AddStudentAcademicInfoAsync(academicInfo, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var saved = await repository.GetStudentByIdAsync(student.Id, cancellationToken)
            ?? throw new NotFoundException("Student not found after conversion.", "student_not_found");

        await auditService.WriteAsync(ModuleCodes.StudentManagement, "StudentCreatedFromAdmission", nameof(Student), student.Id.ToString(), "Success", $"Admission {admission.AdmissionNumber} converted to student.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapStudent(saved);
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        await EnsureStudentDependenciesAsync(schoolId, request.ClassId, request.SectionId, request.AcademicSessionId, cancellationToken);
        await EnsureStudentUniquenessAsync(schoolId, request.ClassId, request.SectionId, request.AcademicSessionId, request.RollNumber.Trim(), request.Email?.Trim(), request.MobileNumber.Trim(), null, cancellationToken);

        var student = new Student
        {
            SchoolId = schoolId,
            AdmissionId = request.AdmissionId,
            RollNumber = request.RollNumber.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Gender = request.Gender,
            DateOfBirthUtc = request.DateOfBirth,
            MobileNumber = request.MobileNumber.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address.Trim(),
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            AcademicSessionId = request.AcademicSessionId,
            AdmissionDateUtc = request.AdmissionDate,
            BloodGroup = request.BloodGroup?.Trim(),
            Religion = request.Religion?.Trim(),
            Category = request.Category?.Trim(),
            AadhaarNumber = request.AadhaarNumber?.Trim(),
            IsActive = true,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        var academicInfo = new StudentAcademicInfo
        {
            SchoolId = schoolId,
            StudentId = student.Id,
            PreviousSchool = request.PreviousSchool?.Trim(),
            Remarks = request.Remarks?.Trim(),
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddStudentAsync(student, cancellationToken);
        await repository.AddStudentAcademicInfoAsync(academicInfo, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var saved = await repository.GetStudentByIdAsync(student.Id, cancellationToken) ?? student;
        await auditService.WriteAsync(ModuleCodes.StudentManagement, "StudentCreated", nameof(Student), student.Id.ToString(), "Success", $"Student {student.RollNumber} created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapStudent(saved);
    }

    public async Task<StudentDto> UpdateAsync(Guid studentId, UpdateStudentRequestDto request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var student = await GetManagedStudentAsync(studentId, cancellationToken);

        await EnsureStudentDependenciesAsync(student.SchoolId, request.ClassId, request.SectionId, request.AcademicSessionId, cancellationToken);
        await EnsureStudentUniquenessAsync(student.SchoolId, request.ClassId, request.SectionId, request.AcademicSessionId, request.RollNumber.Trim(), request.Email?.Trim(), request.MobileNumber.Trim(), student.Id, cancellationToken);

        student.RollNumber = request.RollNumber.Trim();
        student.FirstName = request.FirstName.Trim();
        student.LastName = request.LastName.Trim();
        student.Gender = request.Gender;
        student.DateOfBirthUtc = request.DateOfBirth;
        student.MobileNumber = request.MobileNumber.Trim();
        student.Email = request.Email?.Trim();
        student.Address = request.Address.Trim();
        student.ClassId = request.ClassId;
        student.SectionId = request.SectionId;
        student.AcademicSessionId = request.AcademicSessionId;
        student.AdmissionDateUtc = request.AdmissionDate;
        student.BloodGroup = request.BloodGroup?.Trim();
        student.Religion = request.Religion?.Trim();
        student.Category = request.Category?.Trim();
        student.AadhaarNumber = request.AadhaarNumber?.Trim();
        student.ModifiedAtUtc = DateTime.UtcNow;
        student.ModifiedBy = currentUserContext.UserId?.ToString();

        if (student.AcademicInfo is not null)
        {
            student.AcademicInfo.PreviousSchool = request.PreviousSchool?.Trim();
            student.AcademicInfo.Remarks = request.Remarks?.Trim();
            student.AcademicInfo.ModifiedAtUtc = DateTime.UtcNow;
            student.AcademicInfo.ModifiedBy = currentUserContext.UserId?.ToString();
        }

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetStudentByIdAsync(student.Id, cancellationToken) ?? student;
        await auditService.WriteAsync(ModuleCodes.StudentManagement, "StudentUpdated", nameof(Student), student.Id.ToString(), "Success", $"Student {student.RollNumber} updated.", student.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapStudent(saved);
    }

    public async Task<StudentDto> GetByIdAsync(Guid studentId, CancellationToken cancellationToken)
        => MapStudent(await GetManagedStudentAsync(studentId, cancellationToken));

    public async Task<PagedResult<StudentDto>> GetAllAsync(GetStudentsRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetStudentPageAsync(schoolId, request.Search, request.ClassId, request.SectionId, request.AcademicSessionId, request.IsActive, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<StudentDto>
        {
            Items = items.Select(MapStudent).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StudentDto> PromoteAsync(Guid studentId, PromoteStudentRequestDto request, CancellationToken cancellationToken)
    {
        await promoteValidator.ValidateAndThrowAsync(request, cancellationToken);
        var student = await GetManagedStudentAsync(studentId, cancellationToken);
        await EnsureStudentDependenciesAsync(student.SchoolId, request.TargetClassId, request.TargetSectionId, request.TargetAcademicSessionId, cancellationToken);
        await EnsureStudentUniquenessAsync(student.SchoolId, request.TargetClassId, request.TargetSectionId, request.TargetAcademicSessionId, request.NewRollNumber.Trim(), student.Email, student.MobileNumber, student.Id, cancellationToken);

        student.ClassId = request.TargetClassId;
        student.SectionId = request.TargetSectionId;
        student.AcademicSessionId = request.TargetAcademicSessionId;
        student.RollNumber = request.NewRollNumber.Trim();
        student.ModifiedAtUtc = DateTime.UtcNow;
        student.ModifiedBy = currentUserContext.UserId?.ToString();

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetStudentByIdAsync(student.Id, cancellationToken) ?? student;
        await auditService.WriteAsync(ModuleCodes.StudentManagement, "StudentPromoted", nameof(Student), student.Id.ToString(), "Success", $"Student {student.RollNumber} promoted.", student.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapStudent(saved);
    }

    public async Task<StudentDto> TransferAsync(Guid studentId, TransferStudentRequestDto request, CancellationToken cancellationToken)
    {
        await transferValidator.ValidateAndThrowAsync(request, cancellationToken);
        var student = await GetManagedStudentAsync(studentId, cancellationToken);
        await EnsureStudentDependenciesAsync(student.SchoolId, request.TargetClassId, request.TargetSectionId, request.TargetAcademicSessionId, cancellationToken);
        await EnsureStudentUniquenessAsync(student.SchoolId, request.TargetClassId, request.TargetSectionId, request.TargetAcademicSessionId, request.NewRollNumber.Trim(), student.Email, student.MobileNumber, student.Id, cancellationToken);

        student.ClassId = request.TargetClassId;
        student.SectionId = request.TargetSectionId;
        student.AcademicSessionId = request.TargetAcademicSessionId;
        student.RollNumber = request.NewRollNumber.Trim();
        student.ModifiedAtUtc = DateTime.UtcNow;
        student.ModifiedBy = currentUserContext.UserId?.ToString();

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetStudentByIdAsync(student.Id, cancellationToken) ?? student;
        await auditService.WriteAsync(ModuleCodes.StudentManagement, "StudentTransferred", nameof(Student), student.Id.ToString(), "Success", $"Student {student.RollNumber} transferred.", student.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapStudent(saved);
    }

    public async Task<StudentDto> DeactivateAsync(Guid studentId, DeactivateStudentRequestDto request, CancellationToken cancellationToken)
    {
        var student = await GetManagedStudentAsync(studentId, cancellationToken);
        student.IsActive = false;
        student.ModifiedAtUtc = DateTime.UtcNow;
        student.ModifiedBy = currentUserContext.UserId?.ToString();
        if (student.AcademicInfo is not null && !string.IsNullOrWhiteSpace(request.Remarks))
        {
            student.AcademicInfo.Remarks = request.Remarks.Trim();
            student.AcademicInfo.ModifiedAtUtc = DateTime.UtcNow;
            student.AcademicInfo.ModifiedBy = currentUserContext.UserId?.ToString();
        }

        await repository.SaveChangesAsync(cancellationToken);
        var saved = await repository.GetStudentByIdAsync(student.Id, cancellationToken) ?? student;
        await auditService.WriteAsync(ModuleCodes.StudentManagement, "StudentDeactivated", nameof(Student), student.Id.ToString(), "Success", $"Student {student.RollNumber} deactivated.", student.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapStudent(saved);
    }

    public async Task<StudentDocumentDto> UploadDocumentAsync(Guid studentId, UploadStudentDocumentRequestDto request, CancellationToken cancellationToken)
    {
        await documentValidator.ValidateAndThrowAsync(request, cancellationToken);
        var student = await GetManagedStudentAsync(studentId, cancellationToken);
        FileUploadValidationHelper.Validate(request.File, AllowedDocumentTypes, 5 * 1024 * 1024, "Student document");
        var upload = await fileStorageService.UploadAsync(student.SchoolId, ModuleCodes.StudentManagement, "documents", request.File!, cancellationToken);

        var document = new StudentDocument
        {
            SchoolId = student.SchoolId,
            StudentId = student.Id,
            Title = request.Title.Trim(),
            OriginalFileName = upload.OriginalFileName,
            StoredFileName = upload.StoredFileName,
            ContentType = upload.ContentType,
            FileSize = upload.FileSize,
            FileUrl = upload.FileUrl,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddStudentDocumentAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StudentManagement, "StudentDocumentUploaded", nameof(StudentDocument), document.Id.ToString(), "Success", $"Document uploaded for student {student.RollNumber}.", student.SchoolId, currentUserContext.UserId, cancellationToken);
        return new StudentDocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            OriginalFileName = document.OriginalFileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            FileUrl = document.FileUrl,
            CreatedAt = document.CreatedAtUtc
        };
    }

    private async Task<Student> GetManagedStudentAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await repository.GetStudentByIdAsync(studentId, cancellationToken)
            ?? throw new NotFoundException("Student not found.", "student_not_found");

        EnsureSchoolAccess(student.SchoolId);
        return student;
    }

    private async Task EnsureStudentDependenciesAsync(Guid schoolId, Guid classId, Guid sectionId, Guid academicSessionId, CancellationToken cancellationToken)
    {
        var classEntity = await repository.GetClassByIdAsync(classId, cancellationToken)
            ?? throw new NotFoundException("Class not found.", "class_not_found");
        var section = await repository.GetSectionByIdAsync(sectionId, cancellationToken)
            ?? throw new NotFoundException("Section not found.", "section_not_found");
        var session = await repository.GetAcademicSessionByIdAsync(academicSessionId, cancellationToken)
            ?? throw new NotFoundException("Academic session not found.", "academic_session_not_found");

        if (classEntity.TenantId != schoolId || section.TenantId != schoolId || session.TenantId != schoolId || section.ClassId != classId)
        {
            throw new ForbiddenException("Student access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private async Task EnsureStudentUniquenessAsync(Guid schoolId, Guid classId, Guid sectionId, Guid academicSessionId, string rollNumber, string? email, string mobileNumber, Guid? excludeStudentId, CancellationToken cancellationToken)
    {
        if (await repository.RollNumberExistsAsync(schoolId, classId, sectionId, academicSessionId, rollNumber, excludeStudentId, cancellationToken))
        {
            throw new ConflictException("Roll number must be unique within class, section, and session.", "roll_number_exists");
        }

        if (await repository.StudentMobileExistsAsync(schoolId, mobileNumber, excludeStudentId, cancellationToken))
        {
            throw new ConflictException("Mobile number already exists within the school.", "student_mobile_exists");
        }

        if (!string.IsNullOrWhiteSpace(email) && await repository.StudentEmailExistsAsync(schoolId, email, excludeStudentId, cancellationToken))
        {
            throw new ConflictException("Email already exists within the school.", "student_email_exists");
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
                throw new ForbiddenException("Student access is limited to the current school.", "cross_tenant_access_forbidden");
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
            throw new ForbiddenException("Student access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Student access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static StudentDto MapStudent(Student student) => new()
    {
        Id = student.Id,
        SchoolId = student.SchoolId,
        AdmissionId = student.AdmissionId,
        RollNumber = student.RollNumber,
        FirstName = student.FirstName,
        LastName = student.LastName,
        Gender = student.Gender,
        DateOfBirth = student.DateOfBirthUtc,
        MobileNumber = student.MobileNumber,
        Email = student.Email,
        Address = student.Address,
        ClassId = student.ClassId,
        ClassName = student.Class?.Name ?? string.Empty,
        SectionId = student.SectionId,
        SectionName = student.Section?.Name ?? string.Empty,
        AcademicSessionId = student.AcademicSessionId,
        AcademicSessionName = student.AcademicSession?.Name ?? string.Empty,
        AdmissionDate = student.AdmissionDateUtc,
        BloodGroup = student.BloodGroup,
        Religion = student.Religion,
        Category = student.Category,
        AadhaarNumber = student.AadhaarNumber,
        IsActive = student.IsActive,
        PreviousSchool = student.AcademicInfo?.PreviousSchool,
        Remarks = student.AcademicInfo?.Remarks,
        Documents = student.Documents.Select(x => new StudentDocumentDto
        {
            Id = x.Id,
            Title = x.Title,
            OriginalFileName = x.OriginalFileName,
            ContentType = x.ContentType,
            FileSize = x.FileSize,
            FileUrl = x.FileUrl,
            CreatedAt = x.CreatedAtUtc
        }).ToArray(),
        CreatedAt = student.CreatedAtUtc
    };
}
