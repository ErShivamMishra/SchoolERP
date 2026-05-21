using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Dashboard.Models;

namespace SchoolERP.Application.Features.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<DashboardAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<PagedResult<RecentActivityDto>> GetRecentActivityAsync(DashboardActivityRequestDto request, CancellationToken cancellationToken);
}
