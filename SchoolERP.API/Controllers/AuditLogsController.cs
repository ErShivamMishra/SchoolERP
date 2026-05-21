using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.AuditLogs.Interfaces;
using SchoolERP.Application.Features.AuditLogs.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin}")]
[Route("api/v1/audit-logs")]
public sealed class AuditLogsController(IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    [ModuleAccess(ModuleCodes.DashboardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AuditLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await auditLogService.GetLogsAsync(request, cancellationToken), "Audit logs fetched successfully."));
}
