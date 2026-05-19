using FluentValidation;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Students.Models;

public class CreateStudentRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid? AdmissionId { get; init; }
    public string RollNumber { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string MobileNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Address { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public Guid SectionId { get; init; }
    public Guid AcademicSessionId { get; init; }
    public DateTime AdmissionDate { get; init; }
    public string? BloodGroup { get; init; }
    public string? Religion { get; init; }
    public string? Category { get; init; }
    public string? AadhaarNumber { get; init; }
    public string? PreviousSchool { get; init; }
    public string? Remarks { get; init; }
}

public sealed class UpdateStudentRequestDto : CreateStudentRequestDto
{
}

public sealed class ConvertAdmissionToStudentRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid AdmissionId { get; init; }
    public string RollNumber { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public Guid SectionId { get; init; }
    public Guid AcademicSessionId { get; init; }
    public string? BloodGroup { get; init; }
    public string? Religion { get; init; }
    public string? Category { get; init; }
    public string? AadhaarNumber { get; init; }
}

public sealed class PromoteStudentRequestDto
{
    public Guid TargetClassId { get; init; }
    public Guid TargetSectionId { get; init; }
    public Guid TargetAcademicSessionId { get; init; }
    public string NewRollNumber { get; init; } = string.Empty;
}

public sealed class TransferStudentRequestDto
{
    public Guid TargetClassId { get; init; }
    public Guid TargetSectionId { get; init; }
    public Guid TargetAcademicSessionId { get; init; }
    public string NewRollNumber { get; init; } = string.Empty;
}

public sealed class DeactivateStudentRequestDto
{
    public string? Remarks { get; init; }
}

public sealed class UploadStudentDocumentRequestDto
{
    public string Title { get; init; } = string.Empty;
    public FileUploadPayload? File { get; init; }
}

public sealed class StudentDocumentDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string FileUrl { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class StudentDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public Guid? AdmissionId { get; init; }
    public string RollNumber { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string MobileNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Address { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public Guid AcademicSessionId { get; init; }
    public string AcademicSessionName { get; init; } = string.Empty;
    public DateTime AdmissionDate { get; init; }
    public string? BloodGroup { get; init; }
    public string? Religion { get; init; }
    public string? Category { get; init; }
    public string? AadhaarNumber { get; init; }
    public bool IsActive { get; init; }
    public string? PreviousSchool { get; init; }
    public string? Remarks { get; init; }
    public IReadOnlyCollection<StudentDocumentDto> Documents { get; init; } = Array.Empty<StudentDocumentDto>();
    public DateTime CreatedAt { get; init; }
}

public sealed class GetStudentsRequestDto
{
    public Guid? SchoolId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public Guid? AcademicSessionId { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class CreateStudentRequestDtoValidator : AbstractValidator<CreateStudentRequestDto>
{
    public CreateStudentRequestDtoValidator()
    {
        RuleFor(x => x.RollNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.AcademicSessionId).NotEmpty();
        RuleFor(x => x.BloodGroup).MaximumLength(20);
        RuleFor(x => x.Religion).MaximumLength(100);
        RuleFor(x => x.Category).MaximumLength(100);
        RuleFor(x => x.AadhaarNumber).MaximumLength(30);
        RuleFor(x => x.PreviousSchool).MaximumLength(200);
    }
}

public sealed class UpdateStudentRequestDtoValidator : AbstractValidator<UpdateStudentRequestDto>
{
    public UpdateStudentRequestDtoValidator()
    {
        Include(new CreateStudentRequestDtoValidator());
    }
}

public sealed class ConvertAdmissionToStudentRequestDtoValidator : AbstractValidator<ConvertAdmissionToStudentRequestDto>
{
    public ConvertAdmissionToStudentRequestDtoValidator()
    {
        RuleFor(x => x.AdmissionId).NotEmpty();
        RuleFor(x => x.RollNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.AcademicSessionId).NotEmpty();
    }
}

public sealed class PromoteStudentRequestDtoValidator : AbstractValidator<PromoteStudentRequestDto>
{
    public PromoteStudentRequestDtoValidator()
    {
        RuleFor(x => x.TargetClassId).NotEmpty();
        RuleFor(x => x.TargetSectionId).NotEmpty();
        RuleFor(x => x.TargetAcademicSessionId).NotEmpty();
        RuleFor(x => x.NewRollNumber).NotEmpty().MaximumLength(50);
    }
}

public sealed class TransferStudentRequestDtoValidator : AbstractValidator<TransferStudentRequestDto>
{
    public TransferStudentRequestDtoValidator()
    {
        RuleFor(x => x.TargetClassId).NotEmpty();
        RuleFor(x => x.TargetSectionId).NotEmpty();
        RuleFor(x => x.TargetAcademicSessionId).NotEmpty();
        RuleFor(x => x.NewRollNumber).NotEmpty().MaximumLength(50);
    }
}

public sealed class UploadStudentDocumentRequestDtoValidator : AbstractValidator<UploadStudentDocumentRequestDto>
{
    public UploadStudentDocumentRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
    }
}

public sealed class GetStudentsRequestDtoValidator : AbstractValidator<GetStudentsRequestDto>
{
    public GetStudentsRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
