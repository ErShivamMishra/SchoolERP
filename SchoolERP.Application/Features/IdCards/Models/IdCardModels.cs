using FluentValidation;

namespace SchoolERP.Application.Features.IdCards.Models;

public sealed class CreateIdCardTemplateRequestDto
{
    public Guid? SchoolId { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string SchoolDetails { get; init; } = string.Empty;
    public string? LayoutJson { get; init; }
}

public sealed class GenerateStudentIdCardsRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid TemplateId { get; init; }
    public IReadOnlyCollection<Guid> StudentIds { get; init; } = Array.Empty<Guid>();
    public bool IncludeQrMetadata { get; init; } = true;
}

public sealed class GenerateTeacherIdCardsRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid TemplateId { get; init; }
    public IReadOnlyCollection<Guid> TeacherIds { get; init; } = Array.Empty<Guid>();
    public bool IncludeQrMetadata { get; init; } = true;
}

public sealed class IdCardTemplateDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string SchoolDetails { get; init; } = string.Empty;
    public string? LayoutJson { get; init; }
    public int Version { get; init; }
    public bool IsActive { get; init; }
}

public sealed class GeneratedIdCardDto
{
    public Guid Id { get; init; }
    public Guid TemplateId { get; init; }
    public Guid? StudentId { get; init; }
    public Guid? TeacherId { get; init; }
    public string CardHolderType { get; init; } = string.Empty;
    public string CardHolderName { get; init; } = string.Empty;
    public string CardIdentifier { get; init; } = string.Empty;
    public string? QrCodePayload { get; init; }
    public string? BarcodePayload { get; init; }
    public string SnapshotJson { get; init; } = string.Empty;
}

public sealed class CreateIdCardTemplateRequestDtoValidator : AbstractValidator<CreateIdCardTemplateRequestDto>
{
    public CreateIdCardTemplateRequestDtoValidator()
    {
        RuleFor(x => x.TemplateName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SchoolDetails).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.LogoUrl).MaximumLength(500);
    }
}

public sealed class GenerateStudentIdCardsRequestDtoValidator : AbstractValidator<GenerateStudentIdCardsRequestDto>
{
    public GenerateStudentIdCardsRequestDtoValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.StudentIds).NotEmpty();
    }
}

public sealed class GenerateTeacherIdCardsRequestDtoValidator : AbstractValidator<GenerateTeacherIdCardsRequestDto>
{
    public GenerateTeacherIdCardsRequestDtoValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.TeacherIds).NotEmpty();
    }
}
