using FluentValidation;
using SchoolERP.Application.Common.FileStorage;

namespace SchoolERP.Application.Features.Study.Models;

public class CreateSubjectRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class UpdateSubjectRequestDto : CreateSubjectRequestDto
{
}

public sealed class SubjectDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class UploadSyllabusRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid ClassId { get; init; }
    public Guid AcademicSessionId { get; init; }
    public string Topics { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class SyllabusDto
{
    public Guid Id { get; init; }
    public Guid SubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid AcademicSessionId { get; init; }
    public string AcademicSessionName { get; init; } = string.Empty;
    public string Topics { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class UploadStudyMaterialRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid TeacherId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public FileUploadPayload? File { get; init; }
}

public sealed class StudyMaterialDto
{
    public Guid Id { get; init; }
    public Guid SubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public Guid TeacherId { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string FileUrl { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public DateTime UploadDate { get; init; }
}

public sealed class CreateHomeworkAssignmentRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid TeacherId { get; init; }
    public Guid ClassId { get; init; }
    public Guid SectionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public FileUploadPayload? Attachment { get; init; }
}

public sealed class HomeworkAssignmentDto
{
    public Guid Id { get; init; }
    public Guid SubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public Guid TeacherId { get; init; }
    public string TeacherName { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public DateTime DueDate { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? OriginalFileName { get; init; }
    public string? ContentType { get; init; }
    public long? FileSize { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class GetStudyMaterialsRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SubjectId { get; init; }
}

public sealed class GetHomeworkAssignmentsRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid ClassId { get; init; }
    public Guid? SectionId { get; init; }
}

public sealed class CreateSubjectRequestDtoValidator : AbstractValidator<CreateSubjectRequestDto>
{
    public CreateSubjectRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public sealed class UpdateSubjectRequestDtoValidator : AbstractValidator<UpdateSubjectRequestDto>
{
    public UpdateSubjectRequestDtoValidator()
    {
        Include(new CreateSubjectRequestDtoValidator());
    }
}

public sealed class UploadSyllabusRequestDtoValidator : AbstractValidator<UploadSyllabusRequestDto>
{
    public UploadSyllabusRequestDtoValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.AcademicSessionId).NotEmpty();
        RuleFor(x => x.Topics).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UploadStudyMaterialRequestDtoValidator : AbstractValidator<UploadStudyMaterialRequestDto>
{
    public UploadStudyMaterialRequestDtoValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreateHomeworkAssignmentRequestDtoValidator : AbstractValidator<CreateHomeworkAssignmentRequestDto>
{
    public CreateHomeworkAssignmentRequestDtoValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.TeacherId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Instructions).NotEmpty().MaximumLength(2000);
    }
}
