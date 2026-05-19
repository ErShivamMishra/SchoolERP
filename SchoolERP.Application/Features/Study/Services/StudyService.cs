using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Study.Interfaces;
using SchoolERP.Application.Features.Study.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Study.Services;

public sealed class StudyService(
    IStudyRepository repository,
    IFileStorageService fileStorageService,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateSubjectRequestDto> createSubjectValidator,
    IValidator<UpdateSubjectRequestDto> updateSubjectValidator,
    IValidator<UploadSyllabusRequestDto> syllabusValidator,
    IValidator<UploadStudyMaterialRequestDto> materialValidator,
    IValidator<CreateHomeworkAssignmentRequestDto> homeworkValidator) : IStudyService
{
    private static readonly string[] AllowedMaterialTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation"
    ];

    public async Task<SubjectDto> CreateSubjectAsync(CreateSubjectRequestDto request, CancellationToken cancellationToken)
    {
        await createSubjectValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var code = request.Code.Trim();
        if (await repository.SubjectCodeExistsAsync(schoolId, code, null, cancellationToken))
        {
            throw new ConflictException("Subject code must be unique within the school.", "subject_code_exists");
        }

        var subject = new Subject
        {
            SchoolId = schoolId,
            Name = request.Name.Trim(),
            Code = code,
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddSubjectAsync(subject, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StudyManagement, "SubjectCreated", nameof(Subject), subject.Id.ToString(), "Success", $"Subject {subject.Code} created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapSubject(subject);
    }

    public async Task<SubjectDto> UpdateSubjectAsync(Guid subjectId, UpdateSubjectRequestDto request, CancellationToken cancellationToken)
    {
        await updateSubjectValidator.ValidateAndThrowAsync(request, cancellationToken);
        var subject = await GetManagedSubjectAsync(subjectId, cancellationToken);
        var code = request.Code.Trim();
        if (await repository.SubjectCodeExistsAsync(subject.SchoolId, code, subject.Id, cancellationToken))
        {
            throw new ConflictException("Subject code must be unique within the school.", "subject_code_exists");
        }

        subject.Name = request.Name.Trim();
        subject.Code = code;
        subject.Description = request.Description?.Trim();
        subject.ModifiedAtUtc = DateTime.UtcNow;
        subject.ModifiedBy = currentUserContext.UserId?.ToString();

        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StudyManagement, "SubjectUpdated", nameof(Subject), subject.Id.ToString(), "Success", $"Subject {subject.Code} updated.", subject.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapSubject(subject);
    }

    public async Task<IReadOnlyCollection<SubjectDto>> GetSubjectsAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        return (await repository.GetSubjectsAsync(resolvedSchoolId, cancellationToken)).Select(MapSubject).ToArray();
    }

    public async Task<SyllabusDto> UploadSyllabusAsync(UploadSyllabusRequestDto request, CancellationToken cancellationToken)
    {
        await syllabusValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var subject = await GetSchoolSubjectAsync(schoolId, request.SubjectId, cancellationToken);
        var classEntity = await repository.GetClassByIdAsync(request.ClassId, cancellationToken)
            ?? throw new NotFoundException("Class not found.", "class_not_found");
        var session = await repository.GetAcademicSessionByIdAsync(request.AcademicSessionId, cancellationToken)
            ?? throw new NotFoundException("Academic session not found.", "academic_session_not_found");

        EnsureSchoolOwned(schoolId, classEntity.TenantId);
        EnsureSchoolOwned(schoolId, session.TenantId);

        var syllabus = await repository.GetSyllabusAsync(schoolId, request.SubjectId, request.ClassId, request.AcademicSessionId, cancellationToken);
        if (syllabus is null)
        {
            syllabus = new Syllabus
            {
                SchoolId = schoolId,
                SubjectId = request.SubjectId,
                ClassId = request.ClassId,
                AcademicSessionId = request.AcademicSessionId,
                Topics = request.Topics.Trim(),
                Description = request.Description?.Trim(),
                CreatedBy = currentUserContext.UserId?.ToString()
            };
            await repository.AddSyllabusAsync(syllabus, cancellationToken);
        }
        else
        {
            syllabus.Topics = request.Topics.Trim();
            syllabus.Description = request.Description?.Trim();
            syllabus.ModifiedAtUtc = DateTime.UtcNow;
            syllabus.ModifiedBy = currentUserContext.UserId?.ToString();
        }

        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StudyManagement, "SyllabusUpserted", nameof(Syllabus), syllabus.Id.ToString(), "Success", $"Syllabus updated for subject {subject.Code}.", schoolId, currentUserContext.UserId, cancellationToken);
        return new SyllabusDto
        {
            Id = syllabus.Id,
            SubjectId = subject.Id,
            SubjectName = subject.Name,
            ClassId = classEntity.Id,
            ClassName = classEntity.Name,
            AcademicSessionId = session.Id,
            AcademicSessionName = session.Name,
            Topics = syllabus.Topics,
            Description = syllabus.Description,
            CreatedAt = syllabus.CreatedAtUtc
        };
    }

    public async Task<StudyMaterialDto> UploadStudyMaterialAsync(UploadStudyMaterialRequestDto request, CancellationToken cancellationToken)
    {
        await materialValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var subject = await GetSchoolSubjectAsync(schoolId, request.SubjectId, cancellationToken);
        var teacher = await repository.GetTeacherByIdAsync(request.TeacherId, cancellationToken)
            ?? throw new NotFoundException("Teacher not found.", "teacher_not_found");
        EnsureSchoolOwned(schoolId, teacher.SchoolId);
        FileUploadValidationHelper.Validate(request.File, AllowedMaterialTypes, 10 * 1024 * 1024, "Study material");
        var upload = await fileStorageService.UploadAsync(schoolId, ModuleCodes.StudyManagement, "materials", request.File!, cancellationToken);

        var material = new StudyMaterial
        {
            SchoolId = schoolId,
            SubjectId = request.SubjectId,
            TeacherId = request.TeacherId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            FileUrl = upload.FileUrl,
            OriginalFileName = upload.OriginalFileName,
            StoredFileName = upload.StoredFileName,
            ContentType = upload.ContentType,
            FileSize = upload.FileSize,
            UploadDateUtc = DateTime.UtcNow,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddStudyMaterialAsync(material, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StudyManagement, "StudyMaterialUploaded", nameof(StudyMaterial), material.Id.ToString(), "Success", $"Study material uploaded for subject {subject.Code}.", schoolId, currentUserContext.UserId, cancellationToken);
        return new StudyMaterialDto
        {
            Id = material.Id,
            SubjectId = subject.Id,
            SubjectName = subject.Name,
            TeacherId = teacher.Id,
            TeacherName = $"{teacher.FirstName} {teacher.LastName}".Trim(),
            Title = material.Title,
            Description = material.Description,
            FileUrl = material.FileUrl,
            OriginalFileName = material.OriginalFileName,
            ContentType = material.ContentType,
            FileSize = material.FileSize,
            UploadDate = material.UploadDateUtc
        };
    }

    public async Task<HomeworkAssignmentDto> CreateHomeworkAsync(CreateHomeworkAssignmentRequestDto request, CancellationToken cancellationToken)
    {
        await homeworkValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var subject = await GetSchoolSubjectAsync(schoolId, request.SubjectId, cancellationToken);
        var teacher = await repository.GetTeacherByIdAsync(request.TeacherId, cancellationToken)
            ?? throw new NotFoundException("Teacher not found.", "teacher_not_found");
        var classEntity = await repository.GetClassByIdAsync(request.ClassId, cancellationToken)
            ?? throw new NotFoundException("Class not found.", "class_not_found");
        var section = await repository.GetSectionByIdAsync(request.SectionId, cancellationToken)
            ?? throw new NotFoundException("Section not found.", "section_not_found");

        EnsureSchoolOwned(schoolId, teacher.SchoolId);
        EnsureSchoolOwned(schoolId, classEntity.TenantId);
        EnsureSchoolOwned(schoolId, section.TenantId);
        if (section.ClassId != classEntity.Id)
        {
            throw new BadRequestException("Section does not belong to the selected class.", "invalid_section_class");
        }

        FileUploadResult? upload = null;
        if (request.Attachment is not null && request.Attachment.Content.Length > 0)
        {
            FileUploadValidationHelper.Validate(request.Attachment, AllowedMaterialTypes, 10 * 1024 * 1024, "Homework attachment");
            upload = await fileStorageService.UploadAsync(schoolId, ModuleCodes.StudyManagement, "homework", request.Attachment, cancellationToken);
        }

        var homework = new HomeworkAssignment
        {
            SchoolId = schoolId,
            SubjectId = request.SubjectId,
            TeacherId = request.TeacherId,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            Title = request.Title.Trim(),
            Instructions = request.Instructions.Trim(),
            DueDateUtc = request.DueDate,
            AttachmentUrl = upload?.FileUrl,
            OriginalFileName = upload?.OriginalFileName,
            StoredFileName = upload?.StoredFileName,
            ContentType = upload?.ContentType,
            FileSize = upload?.FileSize,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddHomeworkAssignmentAsync(homework, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StudyManagement, "HomeworkCreated", nameof(HomeworkAssignment), homework.Id.ToString(), "Success", $"Homework created for subject {subject.Code}.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapHomework(homework, subject.Name, teacher, classEntity.Name, section.Name);
    }

    public async Task<IReadOnlyCollection<StudyMaterialDto>> GetStudyMaterialsAsync(GetStudyMaterialsRequestDto request, CancellationToken cancellationToken)
    {
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        return (await repository.GetStudyMaterialsAsync(schoolId, request.ClassId, request.SubjectId, cancellationToken))
            .Select(x => new StudyMaterialDto
            {
                Id = x.Id,
                SubjectId = x.SubjectId,
                SubjectName = x.Subject?.Name ?? string.Empty,
                TeacherId = x.TeacherId,
                TeacherName = x.Teacher is null ? string.Empty : $"{x.Teacher.FirstName} {x.Teacher.LastName}".Trim(),
                Title = x.Title,
                Description = x.Description,
                FileUrl = x.FileUrl,
                OriginalFileName = x.OriginalFileName,
                ContentType = x.ContentType,
                FileSize = x.FileSize,
                UploadDate = x.UploadDateUtc
            }).ToArray();
    }

    public async Task<IReadOnlyCollection<HomeworkAssignmentDto>> GetHomeworkAssignmentsAsync(GetHomeworkAssignmentsRequestDto request, CancellationToken cancellationToken)
    {
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var classEntity = await repository.GetClassByIdAsync(request.ClassId, cancellationToken)
            ?? throw new NotFoundException("Class not found.", "class_not_found");
        EnsureSchoolOwned(schoolId, classEntity.TenantId);

        return (await repository.GetHomeworkAssignmentsAsync(schoolId, request.ClassId, request.SectionId, cancellationToken))
            .Select(x => MapHomework(x, x.Subject?.Name ?? string.Empty, x.Teacher, x.Class?.Name ?? string.Empty, x.Section?.Name ?? string.Empty))
            .ToArray();
    }

    private async Task<Subject> GetManagedSubjectAsync(Guid subjectId, CancellationToken cancellationToken)
    {
        var subject = await repository.GetSubjectByIdAsync(subjectId, cancellationToken)
            ?? throw new NotFoundException("Subject not found.", "subject_not_found");
        EnsureSchoolAccess(subject.SchoolId);
        return subject;
    }

    private async Task<Subject> GetSchoolSubjectAsync(Guid schoolId, Guid subjectId, CancellationToken cancellationToken)
    {
        var subject = await repository.GetSubjectByIdAsync(subjectId, cancellationToken)
            ?? throw new NotFoundException("Subject not found.", "subject_not_found");
        EnsureSchoolOwned(schoolId, subject.SchoolId);
        return subject;
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
                throw new ForbiddenException("Study access is limited to the current school.", "cross_tenant_access_forbidden");
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
            throw new ForbiddenException("Study access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Study access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static void EnsureSchoolOwned(Guid expectedSchoolId, Guid actualSchoolId)
    {
        if (expectedSchoolId != actualSchoolId)
        {
            throw new ForbiddenException("Study access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static SubjectDto MapSubject(Subject subject) => new()
    {
        Id = subject.Id,
        SchoolId = subject.SchoolId,
        Name = subject.Name,
        Code = subject.Code,
        Description = subject.Description,
        IsActive = subject.IsActive,
        CreatedAt = subject.CreatedAtUtc
    };

    private static HomeworkAssignmentDto MapHomework(HomeworkAssignment homework, string subjectName, Teacher? teacher, string className, string sectionName) => new()
    {
        Id = homework.Id,
        SubjectId = homework.SubjectId,
        SubjectName = subjectName,
        TeacherId = homework.TeacherId,
        TeacherName = teacher is null ? string.Empty : $"{teacher.FirstName} {teacher.LastName}".Trim(),
        ClassId = homework.ClassId,
        ClassName = className,
        SectionId = homework.SectionId,
        SectionName = sectionName,
        Title = homework.Title,
        Instructions = homework.Instructions,
        DueDate = homework.DueDateUtc,
        AttachmentUrl = homework.AttachmentUrl,
        OriginalFileName = homework.OriginalFileName,
        ContentType = homework.ContentType,
        FileSize = homework.FileSize,
        CreatedAt = homework.CreatedAtUtc
    };
}
