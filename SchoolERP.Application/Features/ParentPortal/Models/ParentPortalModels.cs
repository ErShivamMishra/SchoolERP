using FluentValidation;

namespace SchoolERP.Application.Features.ParentPortal.Models;

public sealed class CreateParentRequestDto
{
    public Guid? SchoolId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? AlternatePhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? Occupation { get; init; }
}

public sealed class CreateParentResultDto
{
    public ParentDto Parent { get; init; } = new();
    public string TemporaryPassword { get; init; } = string.Empty;
}

public sealed class LinkParentStudentRequestDto
{
    public Guid StudentId { get; init; }
    public string RelationshipType { get; init; } = string.Empty;
    public bool IsPrimaryContact { get; init; }
    public bool CanViewAttendance { get; init; } = true;
    public bool CanViewFees { get; init; } = true;
    public bool CanViewResults { get; init; } = true;
    public bool CanViewHomework { get; init; } = true;
    public bool CanViewNotices { get; init; } = true;
}

public sealed class ParentDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? AlternatePhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? Occupation { get; init; }
    public bool IsActive { get; init; }
}

public sealed class ParentLinkedStudentDto
{
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public string RelationshipType { get; init; } = string.Empty;
    public bool IsPrimaryContact { get; init; }
}

public sealed class ParentAttendanceSummaryDto
{
    public Guid StudentId { get; init; }
    public int PresentDays { get; init; }
    public int AbsentDays { get; init; }
    public int LateDays { get; init; }
    public decimal AttendancePercentage { get; init; }
}

public sealed class ParentFeeStatusDto
{
    public Guid StudentId { get; init; }
    public int TotalInvoices { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal PendingAmount { get; init; }
}

public sealed class ParentResultSummaryDto
{
    public Guid ExamId { get; init; }
    public string ExamTitle { get; init; } = string.Empty;
    public decimal ObtainedMarks { get; init; }
    public decimal TotalMarks { get; init; }
    public decimal Percentage { get; init; }
    public bool IsPublished { get; init; }
}

public sealed class ParentHomeworkDto
{
    public Guid HomeworkId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime AssignedDateUtc { get; init; }
    public DateTime DueDateUtc { get; init; }
    public string SubjectName { get; init; } = string.Empty;
}

public sealed class ParentNoticeDto
{
    public Guid NoticeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime? PublishedAtUtc { get; init; }
    public string AudienceType { get; init; } = string.Empty;
    public string NoticeType { get; init; } = string.Empty;
}

public sealed class CreateParentRequestDtoValidator : AbstractValidator<CreateParentRequestDto>
{
    public CreateParentRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.AlternatePhoneNumber).MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.Occupation).MaximumLength(150);
    }
}

public sealed class LinkParentStudentRequestDtoValidator : AbstractValidator<LinkParentStudentRequestDto>
{
    public LinkParentStudentRequestDtoValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.RelationshipType).NotEmpty().MaximumLength(100);
    }
}
