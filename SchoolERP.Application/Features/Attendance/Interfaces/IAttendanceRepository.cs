using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Attendance.Interfaces;

public interface IAttendanceRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken);
    Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Student>> GetStudentsByIdsAsync(Guid schoolId, Guid classId, Guid sectionId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken);
    Task<AttendanceSession?> GetAttendanceSessionAsync(Guid schoolId, Guid classId, Guid sectionId, DateTime attendanceDate, CancellationToken cancellationToken);
    Task<AttendanceSession?> GetAttendanceSessionByIdAsync(Guid attendanceSessionId, CancellationToken cancellationToken);
    Task<AttendanceRecord?> GetAttendanceRecordByIdAsync(Guid attendanceRecordId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AttendanceSummary>> GetMonthlySummaryAsync(Guid schoolId, int year, int month, Guid? classId, Guid? sectionId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<AttendanceSession> Items, int TotalCount)> GetAttendanceSessionsPageAsync(Guid schoolId, DateTime attendanceDate, Guid? classId, Guid? sectionId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<AttendanceRecord> Items, int TotalCount)> GetStudentHistoryPageAsync(Guid schoolId, Guid studentId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(int TotalSessions, int TotalRecords, int PresentCount, int AbsentCount, int LateCount, int HalfDayCount, IReadOnlyCollection<AttendanceSummary> TrendItems)> GetAnalyticsAsync(Guid schoolId, CancellationToken cancellationToken);
    Task AddAttendanceSessionAsync(AttendanceSession session, CancellationToken cancellationToken);
    Task AddAttendanceRecordsAsync(IEnumerable<AttendanceRecord> records, CancellationToken cancellationToken);
    Task AddOrUpdateAttendanceSummaryAsync(AttendanceSummary summary, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
