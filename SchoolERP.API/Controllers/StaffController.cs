using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Staff.Interfaces;
using SchoolERP.Application.Features.Staff.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin}")]
[Route("api/v1/staff")]
public sealed class StaffController(IStaffService staffService) : ControllerBase
{
    [HttpPost]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.Create, "SchoolAdmin can create staff only for the current tenant. SuperAdmin bypass applies.")]
    [ProducesResponseType(typeof(ApiResponse<StaffDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequestDto request, CancellationToken cancellationToken)
    {
        var response = await staffService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Staff created successfully."));
    }

    [HttpPut("{staffId:guid}")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<StaffDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid staffId, [FromBody] UpdateStaffRequestDto request, CancellationToken cancellationToken)
    {
        var response = await staffService.UpdateAsync(staffId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Staff updated successfully."));
    }

    [HttpDelete("{staffId:guid}")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.Delete)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid staffId, CancellationToken cancellationToken)
    {
        await staffService.DeleteAsync(staffId, cancellationToken);
        return Ok(ApiResponseFactory.Success<object?>(null, "Staff deleted successfully."));
    }

    [HttpPatch("{staffId:guid}/activation")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<StaffDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetActivation(Guid staffId, [FromBody] SetStaffActivationRequestDto request, CancellationToken cancellationToken)
    {
        var response = await staffService.SetActivationAsync(staffId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Staff activation updated successfully."));
    }

    [HttpPost("{staffId:guid}/reset-password")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<ResetStaffPasswordResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(Guid staffId, CancellationToken cancellationToken)
    {
        var response = await staffService.ResetPasswordAsync(staffId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Staff password reset successfully."));
    }

    [HttpGet("{staffId:guid}")]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<StaffDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid staffId, CancellationToken cancellationToken)
    {
        var response = await staffService.GetByIdAsync(staffId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Staff fetched successfully."));
    }

    [HttpGet]
    [ModuleAccess(ModuleCodes.StaffManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StaffDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetStaffListRequestDto request, CancellationToken cancellationToken)
    {
        var response = await staffService.GetAllAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Staff fetched successfully."));
    }
}
