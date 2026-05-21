using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Attendance.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class AttendanceRepository(SchoolErpDbContext dbContext) : IAttendanceRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken)
        => dbContext.Classes.FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);

    public Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken)
        => dbContext.Sections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

    public async Task<IReadOnlyCollection<Student>> GetStudentsByIdsAsync(Guid schoolId, Guid classId, Guid sectionId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken)
        => await dbContext.Students
            .Where(x => x.SchoolId == schoolId && x.ClassId == classId && x.SectionId == sectionId && studentIds.Contains(x.Id) && x.IsActive)
            .ToListAsync(cancellationToken);

    public Task<AttendanceSession?> GetAttendanceSessionAsync(Guid schoolId, Guid classId, Guid sectionId, DateTime attendanceDate, CancellationToken cancellationToken)
        => dbContext.AttendanceSessions
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.Records).ThenInclude(x => x.Student)
            .FirstOrDefaultAsync(x =>
                x.SchoolId == schoolId &&
                x.ClassId == classId &&
                x.SectionId == sectionId &&
                x.AttendanceDate == attendanceDate.Date, cancellationToken);

    public Task<AttendanceSession?> GetAttendanceSessionByIdAsync(Guid attendanceSessionId, CancellationToken cancellationToken)
        => dbContext.AttendanceSessions
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.Records).ThenInclude(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == attendanceSessionId, cancellationToken);

    public Task<AttendanceRecord?> GetAttendanceRecordByIdAsync(Guid attendanceRecordId, CancellationToken cancellationToken)
        => dbContext.AttendanceRecords
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == attendanceRecordId, cancellationToken);

    public async Task<IReadOnlyCollection<AttendanceSummary>> GetMonthlySummaryAsync(Guid schoolId, int year, int month, Guid? classId, Guid? sectionId, CancellationToken cancellationToken)
    {
        var query = dbContext.AttendanceSummaries
            .Include(x => x.Student)
            .Where(x => x.SchoolId == schoolId && x.Year == year && x.Month == month);

        if (classId.HasValue)
        {
            query = query.Where(x => x.Student!.ClassId == classId.Value);
        }

        if (sectionId.HasValue)
        {
            query = query.Where(x => x.Student!.SectionId == sectionId.Value);
        }

        return await query.OrderByDescending(x => x.AttendancePercentage).ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyCollection<AttendanceSession> Items, int TotalCount)> GetAttendanceSessionsPageAsync(Guid schoolId, DateTime attendanceDate, Guid? classId, Guid? sectionId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.AttendanceSessions
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.Records).ThenInclude(x => x.Student)
            .Where(x => x.SchoolId == schoolId && x.AttendanceDate == attendanceDate.Date);

        if (classId.HasValue)
        {
            query = query.Where(x => x.ClassId == classId.Value);
        }

        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Class!.Name.Contains(term) ||
                x.Section!.Name.Contains(term) ||
                (x.SessionName != null && x.SessionName.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Class!.Name)
            .ThenBy(x => x.Section!.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyCollection<AttendanceRecord> Items, int TotalCount)> GetStudentHistoryPageAsync(Guid schoolId, Guid studentId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.AttendanceRecords
            .Include(x => x.Student)
            .Where(x => x.SchoolId == schoolId && x.StudentId == studentId);

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.AttendanceDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.AttendanceDate <= toDate.Value.Date);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.AttendanceDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(int TotalSessions, int TotalRecords, int PresentCount, int AbsentCount, int LateCount, int HalfDayCount, IReadOnlyCollection<AttendanceSummary> TrendItems)> GetAnalyticsAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var totalSessions = await dbContext.AttendanceSessions.CountAsync(x => x.SchoolId == schoolId, cancellationToken);
        var totalRecords = await dbContext.AttendanceRecords.CountAsync(x => x.SchoolId == schoolId, cancellationToken);
        var presentCount = await dbContext.AttendanceRecords.CountAsync(x => x.SchoolId == schoolId && x.Status == AttendanceStatus.Present, cancellationToken);
        var absentCount = await dbContext.AttendanceRecords.CountAsync(x => x.SchoolId == schoolId && x.Status == AttendanceStatus.Absent, cancellationToken);
        var lateCount = await dbContext.AttendanceRecords.CountAsync(x => x.SchoolId == schoolId && x.Status == AttendanceStatus.Late, cancellationToken);
        var halfDayCount = await dbContext.AttendanceRecords.CountAsync(x => x.SchoolId == schoolId && x.Status == AttendanceStatus.HalfDay, cancellationToken);
        var trendItems = await dbContext.AttendanceSummaries
            .Where(x => x.SchoolId == schoolId)
            .GroupBy(x => new { x.Year, x.Month })
            .Select(x => new AttendanceSummary
            {
                SchoolId = schoolId,
                Year = x.Key.Year,
                Month = x.Key.Month,
                AttendancePercentage = x.Average(y => y.AttendancePercentage)
            })
            .ToListAsync(cancellationToken);

        return (totalSessions, totalRecords, presentCount, absentCount, lateCount, halfDayCount, trendItems);
    }

    public Task AddAttendanceSessionAsync(AttendanceSession session, CancellationToken cancellationToken)
        => dbContext.AttendanceSessions.AddAsync(session, cancellationToken).AsTask();

    public Task AddAttendanceRecordsAsync(IEnumerable<AttendanceRecord> records, CancellationToken cancellationToken)
        => dbContext.AttendanceRecords.AddRangeAsync(records, cancellationToken);

    public Task AddOrUpdateAttendanceSummaryAsync(AttendanceSummary summary, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(summary).State == EntityState.Detached)
        {
            dbContext.AttendanceSummaries.Update(summary);
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
