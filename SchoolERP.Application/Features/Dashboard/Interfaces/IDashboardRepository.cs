using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Dashboard.Interfaces;

public interface IDashboardRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<DashboardSnapshot> GetSnapshotAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DashboardMonthlyPoint>> GetAdmissionsTrendAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DashboardMonthlyPoint>> GetRevenueTrendAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DashboardMonthlyPoint>> GetAttendanceTrendAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<AuditLog> Items, int TotalCount)> GetRecentActivityAsync(Guid? schoolId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
}

public sealed class DashboardSnapshot
{
    public int TotalStudents { get; init; }
    public int TotalTeachers { get; init; }
    public int ActiveSchools { get; init; }
    public decimal AttendancePercentage { get; init; }
    public decimal PendingFees { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public decimal QuizParticipation { get; init; }
    public int NewAdmissions { get; init; }
    public int SubscriptionExpiryAlerts { get; init; }
}

public sealed class DashboardMonthlyPoint
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Value { get; init; }
}
