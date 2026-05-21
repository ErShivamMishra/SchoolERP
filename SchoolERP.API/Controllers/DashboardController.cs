using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Dashboard.Interfaces;
using SchoolERP.Application.Features.Dashboard.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("summary")]
    [ModuleAccess(ModuleCodes.DashboardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Summary([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await dashboardService.GetSummaryAsync(schoolId, cancellationToken), "Dashboard summary fetched successfully."));

    [HttpGet("analytics")]
    [ModuleAccess(ModuleCodes.DashboardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<DashboardAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Analytics([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await dashboardService.GetAnalyticsAsync(schoolId, cancellationToken), "Dashboard analytics fetched successfully."));

    [HttpGet("recent-activity")]
    [ModuleAccess(ModuleCodes.DashboardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RecentActivityDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecentActivity([FromQuery] DashboardActivityRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await dashboardService.GetRecentActivityAsync(request, cancellationToken), "Dashboard recent activity fetched successfully."));
}
