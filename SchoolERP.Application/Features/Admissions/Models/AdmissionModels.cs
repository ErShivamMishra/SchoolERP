using FluentValidation;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Admissions.Models;

public sealed class CreateAcademicSessionRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public sealed class AcademicSessionDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CreateClassRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class ClassDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CreateSectionRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid ClassId { get; init; }
    public string Name { get; init; } = string.Empty;
}

public sealed class SectionDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class CreateAdmissionRequestDto
{
    public Guid? SchoolId { get; init; }
    public string AdmissionNumber { get; init; } = string.Empty;
    public string StudentFirstName { get; init; } = string.Empty;
    public string StudentLastName { get; init; } = string.Empty;
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string MobileNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Address { get; init; } = string.Empty;
    public string? PreviousSchool { get; init; }
    public string GuardianName { get; init; } = string.Empty;
    public string GuardianPhone { get; init; } = string.Empty;
    public string GuardianRelation { get; init; } = string.Empty;
    public Guid AppliedClassId { get; init; }
    public Guid AcademicSessionId { get; init; }
    public DateTime AdmissionDate { get; init; }
    public string? Remarks { get; init; }
}

public sealed class UpdateAdmissionRequestDto : CreateAdmissionRequestDto
{
}

public sealed class ChangeAdmissionStatusRequestDto
{
    public string? Remarks { get; init; }
}

public sealed class AdmissionDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string AdmissionNumber { get; init; } = string.Empty;
    public string StudentFirstName { get; init; } = string.Empty;
    public string StudentLastName { get; init; } = string.Empty;
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string MobileNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Address { get; init; } = string.Empty;
    public string? PreviousSchool { get; init; }
    public string GuardianName { get; init; } = string.Empty;
    public string GuardianPhone { get; init; } = string.Empty;
    public string GuardianRelation { get; init; } = string.Empty;
    public Guid AppliedClassId { get; init; }
    public string AppliedClassName { get; init; } = string.Empty;
    public Guid AcademicSessionId { get; init; }
    public string AcademicSessionName { get; init; } = string.Empty;
    public DateTime AdmissionDate { get; init; }
    public AdmissionStatus Status { get; init; }
    public string? Remarks { get; init; }
    public bool IsConvertedToStudent { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class GetAdmissionsRequestDto
{
    public Guid? SchoolId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public AdmissionStatus? Status { get; init; }
    public Guid? AppliedClassId { get; init; }
    public Guid? AcademicSessionId { get; init; }
}

public sealed class CreateAcademicSessionRequestDtoValidator : AbstractValidator<CreateAcademicSessionRequestDto>
{
    public CreateAcademicSessionRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
    }
}

public sealed class CreateClassRequestDtoValidator : AbstractValidator<CreateClassRequestDto>
{
    public CreateClassRequestDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

public sealed class CreateSectionRequestDtoValidator : AbstractValidator<CreateSectionRequestDto>
{
    public CreateSectionRequestDtoValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}

public sealed class CreateAdmissionRequestDtoValidator : AbstractValidator<CreateAdmissionRequestDto>
{
    public CreateAdmissionRequestDtoValidator()
    {
        RuleFor(x => x.AdmissionNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StudentFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StudentLastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PreviousSchool).MaximumLength(200);
        RuleFor(x => x.GuardianName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.GuardianPhone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.GuardianRelation).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AppliedClassId).NotEmpty();
        RuleFor(x => x.AcademicSessionId).NotEmpty();
    }
}

public sealed class UpdateAdmissionRequestDtoValidator : AbstractValidator<UpdateAdmissionRequestDto>
{
    public UpdateAdmissionRequestDtoValidator()
    {
        Include(new CreateAdmissionRequestDtoValidator());
    }
}

public sealed class GetAdmissionsRequestDtoValidator : AbstractValidator<GetAdmissionsRequestDto>
{
    public GetAdmissionsRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
