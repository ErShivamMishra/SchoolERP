using FluentValidation;
using SchoolERP.Application.Common.Models;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Attendance.Models;

public sealed class AttendanceEntryDto
{
    public Guid StudentId { get; init; }
    public AttendanceStatus Status { get; init; }
    public string? Remarks { get; init; }
}

public sealed class MarkAttendanceRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid ClassId { get; init; }
    public Guid SectionId { get; init; }
    public DateTime AttendanceDate { get; init; }
    public string? SessionName { get; init; }
    public IReadOnlyCollection<AttendanceEntryDto> Records { get; init; } = Array.Empty<AttendanceEntryDto>();
}

public sealed class UpdateAttendanceRequestDto
{
    public AttendanceStatus Status { get; init; }
    public string? Remarks { get; init; }
}

public sealed class AttendanceRecordDto
{
    public Guid Id { get; init; }
    public Guid AttendanceSessionId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public Guid SectionId { get; init; }
    public DateTime AttendanceDate { get; init; }
    public AttendanceStatus Status { get; init; }
    public string? Remarks { get; init; }
    public Guid MarkedByUserId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class AttendanceSessionDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public DateTime AttendanceDate { get; init; }
    public string? SessionName { get; init; }
    public int TotalStudents { get; init; }
    public int PresentCount { get; init; }
    public int AbsentCount { get; init; }
    public int LateCount { get; init; }
    public int HalfDayCount { get; init; }
    public decimal AttendancePercentage { get; init; }
    public IReadOnlyCollection<AttendanceRecordDto> Records { get; init; } = Array.Empty<AttendanceRecordDto>();
}

public sealed class DailyAttendanceReportRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public DateTime AttendanceDate { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
}

public sealed class StudentAttendanceHistoryRequestDto : SearchablePagedRequest
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed class MonthlyAttendanceSummaryRequestDto
{
    public Guid? SchoolId { get; init; }
    public int Year { get; init; }
    public int Month { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
}

public sealed class AttendanceSummaryDto
{
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }
    public int TotalDays { get; init; }
    public int PresentDays { get; init; }
    public int AbsentDays { get; init; }
    public int LateDays { get; init; }
    public int HalfDays { get; init; }
    public decimal AttendancePercentage { get; init; }
}

public sealed class AttendanceAnalyticsDto
{
    public decimal OverallAttendancePercentage { get; init; }
    public int TotalSessions { get; init; }
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int AbsentCount { get; init; }
    public int LateCount { get; init; }
    public int HalfDayCount { get; init; }
    public IReadOnlyCollection<AttendanceTrendPointDto> MonthlyTrend { get; init; } = Array.Empty<AttendanceTrendPointDto>();
}

public sealed class AttendanceTrendPointDto
{
    public string Label { get; init; } = string.Empty;
    public decimal AttendancePercentage { get; init; }
}

public sealed class MarkAttendanceRequestDtoValidator : AbstractValidator<MarkAttendanceRequestDto>
{
    public MarkAttendanceRequestDtoValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.AttendanceDate).NotEmpty();
        RuleFor(x => x.Records).NotEmpty();
        RuleForEach(x => x.Records).ChildRules(records =>
        {
            records.RuleFor(x => x.StudentId).NotEmpty();
            records.RuleFor(x => x.Remarks).MaximumLength(500);
        });
    }
}

public sealed class UpdateAttendanceRequestDtoValidator : AbstractValidator<UpdateAttendanceRequestDto>
{
    public UpdateAttendanceRequestDtoValidator()
    {
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

public sealed class DailyAttendanceReportRequestDtoValidator : SearchablePagedRequestValidator<DailyAttendanceReportRequestDto>
{
}

public sealed class StudentAttendanceHistoryRequestDtoValidator : SearchablePagedRequestValidator<StudentAttendanceHistoryRequestDto>
{
}

public sealed class MonthlyAttendanceSummaryRequestDtoValidator : AbstractValidator<MonthlyAttendanceSummaryRequestDto>
{
    public MonthlyAttendanceSummaryRequestDtoValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
