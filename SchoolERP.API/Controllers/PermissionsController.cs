using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.AccessControl.Interfaces;
using SchoolERP.Application.Features.AccessControl.Models;
using SchoolERP.Application.Features.Modules.Interfaces;
using SchoolERP.Application.Features.Modules.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin}")]
[Route("api/v1/permissions")]
public sealed class PermissionsController(
    IPermissionManagementService permissionManagementService,
    IModuleService moduleService) : ControllerBase
{
    [HttpPut("users/{userId:guid}")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ModulePermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignUserPermissions(Guid userId, [FromBody] UpsertUserPermissionsRequestDto request, CancellationToken cancellationToken)
    {
        var response = await permissionManagementService.AssignUserPermissionsAsync(userId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "User permissions updated successfully."));
    }

    [HttpGet("users/{userId:guid}")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ModulePermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
    {
        var response = await permissionManagementService.GetUserPermissionsAsync(userId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "User permissions fetched successfully."));
    }

    [HttpGet("roles/{roleId:guid}")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ModulePermissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolePermissions(Guid roleId, CancellationToken cancellationToken)
    {
        var response = await permissionManagementService.GetRolePermissionsAsync(roleId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Role permissions fetched successfully."));
    }

    [HttpGet("modules")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ModuleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModules(CancellationToken cancellationToken)
    {
        var response = await moduleService.GetAllAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Modules fetched successfully."));
    }
}
