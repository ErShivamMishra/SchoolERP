using FluentValidation;

namespace SchoolERP.Application.Features.Teachers.Models;

public class CreateTeacherRequestDto
{
    public Guid? SchoolId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public Domain.Enums.Gender Gender { get; init; }
    public string LastName { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Qualification { get; init; }
    public int ExperienceYears { get; init; }
    public DateTime JoiningDate { get; init; }
    public string Address { get; init; } = string.Empty;
}

public sealed class UpdateTeacherRequestDto : CreateTeacherRequestDto
{
}

public sealed class AssignTeacherSubjectsRequestDto
{
    public IReadOnlyCollection<Guid> SubjectIds { get; init; } = Array.Empty<Guid>();
}

public sealed class TeacherClassAssignmentRequestDto
{
    public Guid ClassId { get; init; }
    public Guid SectionId { get; init; }
    public Guid AcademicSessionId { get; init; }
}

public sealed class AssignTeacherClassesRequestDto
{
    public IReadOnlyCollection<TeacherClassAssignmentRequestDto> Assignments { get; init; } = Array.Empty<TeacherClassAssignmentRequestDto>();
}

public sealed class DeactivateTeacherRequestDto
{
    public string? Remarks { get; init; }
}

public sealed class TeacherSubjectDto
{
    public Guid SubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public string SubjectCode { get; init; } = string.Empty;
}

public sealed class TeacherClassAssignmentDto
{
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public Guid AcademicSessionId { get; init; }
    public string AcademicSessionName { get; init; } = string.Empty;
}

public sealed class TeacherDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public Domain.Enums.Gender Gender { get; init; }
    public string MobileNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Qualification { get; init; }
    public int ExperienceYears { get; init; }
    public DateTime JoiningDate { get; init; }
    public string Address { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyCollection<TeacherSubjectDto> Subjects { get; init; } = Array.Empty<TeacherSubjectDto>();
    public IReadOnlyCollection<TeacherClassAssignmentDto> ClassAssignments { get; init; } = Array.Empty<TeacherClassAssignmentDto>();
    public DateTime CreatedAt { get; init; }
}

public sealed class GetTeachersRequestDto
{
    public Guid? SchoolId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class CreateTeacherRequestDtoValidator : AbstractValidator<CreateTeacherRequestDto>
{
    public CreateTeacherRequestDtoValidator()
    {
        RuleFor(x => x.EmployeeCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MobileNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Qualification).MaximumLength(150);
        RuleFor(x => x.ExperienceYears).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
    }
}

public sealed class UpdateTeacherRequestDtoValidator : AbstractValidator<UpdateTeacherRequestDto>
{
    public UpdateTeacherRequestDtoValidator()
    {
        Include(new CreateTeacherRequestDtoValidator());
    }
}

public sealed class AssignTeacherSubjectsRequestDtoValidator : AbstractValidator<AssignTeacherSubjectsRequestDto>
{
    public AssignTeacherSubjectsRequestDtoValidator()
    {
        RuleFor(x => x.SubjectIds).NotEmpty();
    }
}

public sealed class AssignTeacherClassesRequestDtoValidator : AbstractValidator<AssignTeacherClassesRequestDto>
{
    public AssignTeacherClassesRequestDtoValidator()
    {
        RuleFor(x => x.Assignments).NotEmpty();
        RuleForEach(x => x.Assignments).ChildRules(assignment =>
        {
            assignment.RuleFor(x => x.ClassId).NotEmpty();
            assignment.RuleFor(x => x.SectionId).NotEmpty();
            assignment.RuleFor(x => x.AcademicSessionId).NotEmpty();
        });
    }
}

public sealed class GetTeachersRequestDtoValidator : AbstractValidator<GetTeachersRequestDto>
{
    public GetTeachersRequestDtoValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
