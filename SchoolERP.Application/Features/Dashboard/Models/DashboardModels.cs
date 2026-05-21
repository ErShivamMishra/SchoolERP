using FluentValidation;
using SchoolERP.Application.Common.Models;

namespace SchoolERP.Application.Features.Dashboard.Models;

public sealed class DashboardSummaryDto
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

public sealed class DashboardTrendPointDto
{
    public string Label { get; init; } = string.Empty;
    public decimal Value { get; init; }
}

public sealed class DashboardAnalyticsDto
{
    public IReadOnlyCollection<DashboardTrendPointDto> AdmissionsTrend { get; init; } = Array.Empty<DashboardTrendPointDto>();
    public IReadOnlyCollection<DashboardTrendPointDto> RevenueTrend { get; init; } = Array.Empty<DashboardTrendPointDto>();
    public IReadOnlyCollection<DashboardTrendPointDto> AttendanceTrend { get; init; } = Array.Empty<DashboardTrendPointDto>();
}

public sealed class RecentActivityDto
{
    public string Module { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class DashboardActivityRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
}

public sealed class DashboardActivityRequestDtoValidator : SearchablePagedRequestValidator<DashboardActivityRequestDto>
{
}
