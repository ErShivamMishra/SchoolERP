using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Attendance.Models;

namespace SchoolERP.Application.Features.Attendance.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceSessionDto> MarkAttendanceAsync(MarkAttendanceRequestDto request, CancellationToken cancellationToken);
    Task<AttendanceSessionDto> BulkMarkAttendanceAsync(MarkAttendanceRequestDto request, CancellationToken cancellationToken);
    Task<AttendanceRecordDto> UpdateAttendanceAsync(Guid attendanceRecordId, UpdateAttendanceRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<AttendanceSessionDto>> GetDailyReportAsync(DailyAttendanceReportRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<AttendanceRecordDto>> GetStudentHistoryAsync(Guid studentId, StudentAttendanceHistoryRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AttendanceSummaryDto>> GetMonthlySummaryAsync(MonthlyAttendanceSummaryRequestDto request, CancellationToken cancellationToken);
    Task<AttendanceAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken);
}
