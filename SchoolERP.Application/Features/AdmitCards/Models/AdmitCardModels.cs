using FluentValidation;

namespace SchoolERP.Application.Features.AdmitCards.Models;

public sealed class CreateAdmitCardTemplateRequestDto
{
    public Guid? SchoolId { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string SchoolDetails { get; init; } = string.Empty;
    public string? LayoutJson { get; init; }
}

public sealed class GenerateAdmitCardsRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid TemplateId { get; init; }
    public Guid ExamId { get; init; }
    public string? Instructions { get; init; }
    public IReadOnlyCollection<GenerateAdmitCardStudentEntryDto> Students { get; init; } = Array.Empty<GenerateAdmitCardStudentEntryDto>();
}

public sealed class GenerateAdmitCardStudentEntryDto
{
    public Guid StudentId { get; init; }
    public string SeatNumber { get; init; } = string.Empty;
    public string? RoomNumber { get; init; }
}

public sealed class AdmitCardTemplateDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public string SchoolDetails { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public int Version { get; init; }
}

public sealed class GeneratedAdmitCardDto
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public string SeatNumber { get; init; } = string.Empty;
    public string? RoomNumber { get; init; }
    public string SnapshotJson { get; init; } = string.Empty;
}

public sealed class CreateAdmitCardTemplateRequestDtoValidator : AbstractValidator<CreateAdmitCardTemplateRequestDto>
{
    public CreateAdmitCardTemplateRequestDtoValidator()
    {
        RuleFor(x => x.TemplateName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SchoolDetails).NotEmpty().MaximumLength(1000);
    }
}

public sealed class GenerateAdmitCardsRequestDtoValidator : AbstractValidator<GenerateAdmitCardsRequestDto>
{
    public GenerateAdmitCardsRequestDtoValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.Students).NotEmpty();
        RuleForEach(x => x.Students).ChildRules(student =>
        {
            student.RuleFor(x => x.StudentId).NotEmpty();
            student.RuleFor(x => x.SeatNumber).NotEmpty().MaximumLength(50);
            student.RuleFor(x => x.RoomNumber).MaximumLength(100);
        });
    }
}
