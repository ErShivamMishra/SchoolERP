using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Attendance.Interfaces;
using SchoolERP.Application.Features.Attendance.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using System.Text.Json;

namespace SchoolERP.Application.Features.Attendance.Services;

public sealed class AttendanceService(
    IAttendanceRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<MarkAttendanceRequestDto> markValidator,
    IValidator<UpdateAttendanceRequestDto> updateValidator,
    IValidator<DailyAttendanceReportRequestDto> reportValidator,
    IValidator<StudentAttendanceHistoryRequestDto> historyValidator,
    IValidator<MonthlyAttendanceSummaryRequestDto> summaryValidator) : IAttendanceService
{
    public Task<AttendanceSessionDto> BulkMarkAttendanceAsync(MarkAttendanceRequestDto request, CancellationToken cancellationToken)
        => MarkAttendanceInternalAsync(request, true, cancellationToken);

    public Task<AttendanceSessionDto> MarkAttendanceAsync(MarkAttendanceRequestDto request, CancellationToken cancellationToken)
        => MarkAttendanceInternalAsync(request, false, cancellationToken);

    public async Task<AttendanceRecordDto> UpdateAttendanceAsync(Guid attendanceRecordId, UpdateAttendanceRequestDto request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var record = await repository.GetAttendanceRecordByIdAsync(attendanceRecordId, cancellationToken)
            ?? throw new NotFoundException("Attendance record not found.", "attendance_record_not_found");

        EnsureSchoolAccess(record.SchoolId);

        var oldValues = JsonSerializer.Serialize(new { record.Status, record.Remarks });
        record.Status = request.Status;
        record.Remarks = request.Remarks?.Trim();
        record.ModifiedAtUtc = DateTime.UtcNow;
        record.ModifiedBy = currentUserContext.UserId?.ToString();

        await repository.SaveChangesAsync(cancellationToken);
        await RefreshMonthlySummaryAsync(record.SchoolId, record.StudentId, record.AttendanceDate, cancellationToken);

        await auditService.WriteAsync(
            ModuleCodes.AttendanceManagement,
            "AttendanceUpdated",
            nameof(AttendanceRecord),
            record.Id.ToString(),
            "Success",
            "Attendance record updated.",
            record.SchoolId,
            currentUserContext.UserId,
            oldValues,
            JsonSerializer.Serialize(new { record.Status, record.Remarks }),
            null,
            null,
            cancellationToken);

        return MapRecord(record);
    }

    public async Task<PagedResult<AttendanceSessionDto>> GetDailyReportAsync(DailyAttendanceReportRequestDto request, CancellationToken cancellationToken)
    {
        await reportValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await repository.GetAttendanceSessionsPageAsync(
            schoolId,
            request.AttendanceDate.Date,
            request.ClassId,
            request.SectionId,
            request.Search,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return new PagedResult<AttendanceSessionDto>
        {
            Items = items.Select(MapSession).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<AttendanceRecordDto>> GetStudentHistoryAsync(Guid studentId, StudentAttendanceHistoryRequestDto request, CancellationToken cancellationToken)
    {
        await historyValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(null);
        var (items, totalCount) = await repository.GetStudentHistoryPageAsync(schoolId, studentId, request.FromDate, request.ToDate, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<AttendanceRecordDto>
        {
            Items = items.Select(MapRecord).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyCollection<AttendanceSummaryDto>> GetMonthlySummaryAsync(MonthlyAttendanceSummaryRequestDto request, CancellationToken cancellationToken)
    {
        await summaryValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var items = await repository.GetMonthlySummaryAsync(schoolId, request.Year, request.Month, request.ClassId, request.SectionId, cancellationToken);
        return items.Select(x => new AttendanceSummaryDto
        {
            StudentId = x.StudentId,
            StudentName = $"{x.Student?.FirstName} {x.Student?.LastName}".Trim(),
            RollNumber = x.Student?.RollNumber ?? string.Empty,
            Year = x.Year,
            Month = x.Month,
            TotalDays = x.TotalDays,
            PresentDays = x.PresentDays,
            AbsentDays = x.AbsentDays,
            LateDays = x.LateDays,
            HalfDays = x.HalfDays,
            AttendancePercentage = x.AttendancePercentage
        }).ToArray();
    }

    public async Task<AttendanceAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = ResolveSchoolIdForRead(schoolId);
        var analytics = await repository.GetAnalyticsAsync(resolvedSchoolId, cancellationToken);
        var denominator = analytics.PresentCount + analytics.AbsentCount + analytics.LateCount + analytics.HalfDayCount;
        return new AttendanceAnalyticsDto
        {
            OverallAttendancePercentage = denominator == 0 ? 0 : decimal.Round(((analytics.PresentCount + analytics.HalfDayCount * 0.5m) / denominator) * 100m, 2),
            TotalSessions = analytics.TotalSessions,
            TotalRecords = analytics.TotalRecords,
            PresentCount = analytics.PresentCount,
            AbsentCount = analytics.AbsentCount,
            LateCount = analytics.LateCount,
            HalfDayCount = analytics.HalfDayCount,
            MonthlyTrend = analytics.TrendItems
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Select(x => new AttendanceTrendPointDto
                {
                    Label = $"{x.Year:D4}-{x.Month:D2}",
                    AttendancePercentage = x.AttendancePercentage
                })
                .ToArray()
        };
    }

    private async Task<AttendanceSessionDto> MarkAttendanceInternalAsync(MarkAttendanceRequestDto request, bool isBulk, CancellationToken cancellationToken)
    {
        await markValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        await EnsureDependenciesAsync(schoolId, request.ClassId, request.SectionId, cancellationToken);

        var existingSession = await repository.GetAttendanceSessionAsync(schoolId, request.ClassId, request.SectionId, request.AttendanceDate.Date, cancellationToken);
        if (existingSession is not null)
        {
            throw new ConflictException("Attendance has already been marked for this class and date.", "attendance_already_marked");
        }

        var studentIds = request.Records.Select(x => x.StudentId).Distinct().ToArray();
        var students = await repository.GetStudentsByIdsAsync(schoolId, request.ClassId, request.SectionId, studentIds, cancellationToken);
        if (students.Count != studentIds.Length)
        {
            throw new BadRequestException("One or more students are invalid for the selected class and section.", "invalid_attendance_students");
        }

        if (!currentUserContext.UserId.HasValue)
        {
            throw new UnauthorizedException("Authentication is required.", "authentication_required");
        }

        var userId = currentUserContext.UserId.Value;

        var session = new AttendanceSession
        {
            SchoolId = schoolId,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            AttendanceDate = request.AttendanceDate.Date,
            SessionName = request.SessionName?.Trim(),
            MarkedByUserId = userId,
            TotalStudents = request.Records.Count,
            PresentCount = request.Records.Count(x => x.Status == AttendanceStatus.Present),
            AbsentCount = request.Records.Count(x => x.Status == AttendanceStatus.Absent),
            LateCount = request.Records.Count(x => x.Status == AttendanceStatus.Late),
            HalfDayCount = request.Records.Count(x => x.Status == AttendanceStatus.HalfDay),
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        var records = request.Records.Select(x => new AttendanceRecord
        {
            SchoolId = schoolId,
            AttendanceSessionId = session.Id,
            StudentId = x.StudentId,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            AttendanceDate = request.AttendanceDate.Date,
            Status = x.Status,
            Remarks = x.Remarks?.Trim(),
            MarkedByUserId = userId,
            CreatedBy = currentUserContext.UserId?.ToString()
        }).ToArray();

        await repository.AddAttendanceSessionAsync(session, cancellationToken);
        await repository.AddAttendanceRecordsAsync(records, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        foreach (var studentId in studentIds)
        {
            await RefreshMonthlySummaryAsync(schoolId, studentId, request.AttendanceDate.Date, cancellationToken);
        }

        var savedSession = await repository.GetAttendanceSessionByIdAsync(session.Id, cancellationToken) ?? session;
        await auditService.WriteAsync(
            ModuleCodes.AttendanceManagement,
            isBulk ? "AttendanceBulkMarked" : "AttendanceMarked",
            nameof(AttendanceSession),
            session.Id.ToString(),
            "Success",
            $"Attendance captured for {request.AttendanceDate:yyyy-MM-dd}.",
            schoolId,
            currentUserContext.UserId,
            null,
            JsonSerializer.Serialize(new { request.ClassId, request.SectionId, request.AttendanceDate, Count = request.Records.Count }),
            null,
            null,
            cancellationToken);

        return MapSession(savedSession);
    }

    private async Task RefreshMonthlySummaryAsync(Guid schoolId, Guid studentId, DateTime attendanceDate, CancellationToken cancellationToken)
    {
        var history = await repository.GetStudentHistoryPageAsync(schoolId, studentId, new DateTime(attendanceDate.Year, attendanceDate.Month, 1), new DateTime(attendanceDate.Year, attendanceDate.Month, DateTime.DaysInMonth(attendanceDate.Year, attendanceDate.Month)), 1, 500, cancellationToken);
        var records = history.Items;
        var totalDays = records.Select(x => x.AttendanceDate.Date).Distinct().Count();
        var presentDays = records.Count(x => x.Status == AttendanceStatus.Present);
        var absentDays = records.Count(x => x.Status == AttendanceStatus.Absent);
        var lateDays = records.Count(x => x.Status == AttendanceStatus.Late);
        var halfDays = records.Count(x => x.Status == AttendanceStatus.HalfDay);

        var existing = (await repository.GetMonthlySummaryAsync(schoolId, attendanceDate.Year, attendanceDate.Month, null, null, cancellationToken))
            .FirstOrDefault(x => x.StudentId == studentId);

        var summary = existing ?? new AttendanceSummary
        {
            SchoolId = schoolId,
            StudentId = studentId,
            Year = attendanceDate.Year,
            Month = attendanceDate.Month,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        summary.TotalDays = totalDays;
        summary.PresentDays = presentDays;
        summary.AbsentDays = absentDays;
        summary.LateDays = lateDays;
        summary.HalfDays = halfDays;
        summary.AttendancePercentage = totalDays == 0 ? 0 : decimal.Round(((presentDays + (halfDays * 0.5m)) / totalDays) * 100m, 2);
        summary.ModifiedAtUtc = DateTime.UtcNow;
        summary.ModifiedBy = currentUserContext.UserId?.ToString();

        await repository.AddOrUpdateAttendanceSummaryAsync(summary, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDependenciesAsync(Guid schoolId, Guid classId, Guid sectionId, CancellationToken cancellationToken)
    {
        var classEntity = await repository.GetClassByIdAsync(classId, cancellationToken)
            ?? throw new NotFoundException("Class not found.", "class_not_found");
        var section = await repository.GetSectionByIdAsync(sectionId, cancellationToken)
            ?? throw new NotFoundException("Section not found.", "section_not_found");

        if (classEntity.TenantId != schoolId || section.TenantId != schoolId || section.ClassId != classId)
        {
            throw new ForbiddenException("Attendance access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private async Task<Guid> ResolveSchoolIdAsync(Guid? requestedSchoolId, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            var schoolId = requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
            _ = await repository.GetSchoolByIdAsync(schoolId, cancellationToken) ?? throw new NotFoundException("School not found.", "school_not_found");
            return schoolId;
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }

        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Attendance access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
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
            throw new ForbiddenException("Attendance access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin) && currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Attendance access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static AttendanceSessionDto MapSession(AttendanceSession session) => new()
    {
        Id = session.Id,
        SchoolId = session.SchoolId,
        ClassId = session.ClassId,
        ClassName = session.Class?.Name ?? string.Empty,
        SectionId = session.SectionId,
        SectionName = session.Section?.Name ?? string.Empty,
        AttendanceDate = session.AttendanceDate,
        SessionName = session.SessionName,
        TotalStudents = session.TotalStudents,
        PresentCount = session.PresentCount,
        AbsentCount = session.AbsentCount,
        LateCount = session.LateCount,
        HalfDayCount = session.HalfDayCount,
        AttendancePercentage = session.TotalStudents == 0 ? 0 : decimal.Round(((session.PresentCount + session.HalfDayCount * 0.5m) / session.TotalStudents) * 100m, 2),
        Records = session.Records.Select(MapRecord).ToArray()
    };

    private static AttendanceRecordDto MapRecord(AttendanceRecord record) => new()
    {
        Id = record.Id,
        AttendanceSessionId = record.AttendanceSessionId,
        StudentId = record.StudentId,
        StudentName = $"{record.Student?.FirstName} {record.Student?.LastName}".Trim(),
        RollNumber = record.Student?.RollNumber ?? string.Empty,
        ClassId = record.ClassId,
        SectionId = record.SectionId,
        AttendanceDate = record.AttendanceDate,
        Status = record.Status,
        Remarks = record.Remarks,
        MarkedByUserId = record.MarkedByUserId,
        CreatedAt = record.CreatedAtUtc
    };
}
