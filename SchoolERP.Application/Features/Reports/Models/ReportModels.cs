using FluentValidation;
using SchoolERP.Application.Common.Models;

namespace SchoolERP.Application.Features.Reports.Models;

public sealed class StudentExportRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
}

public sealed class AttendanceExportRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class QuizResultExportRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public Guid QuizId { get; init; }
}

public sealed class StudentExportRowDto
{
    public string RollNumber { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public string ClassName { get; init; } = string.Empty;
    public string SectionName { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool IsActive { get; init; }
}

public sealed class AttendanceExportRowDto
{
    public DateTime AttendanceDate { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public string ClassName { get; init; } = string.Empty;
    public string SectionName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Remarks { get; init; }
}

public sealed class QuizResultExportRowDto
{
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public decimal ObtainedMarks { get; init; }
    public decimal TotalMarks { get; init; }
    public decimal Percentage { get; init; }
    public bool IsPassed { get; init; }
    public int Rank { get; init; }
}

public sealed class StudentExportRequestDtoValidator : SearchablePagedRequestValidator<StudentExportRequestDto>
{
}

public sealed class AttendanceExportRequestDtoValidator : SearchablePagedRequestValidator<AttendanceExportRequestDto>
{
}

public sealed class QuizResultExportRequestDtoValidator : SearchablePagedRequestValidator<QuizResultExportRequestDto>
{
}
