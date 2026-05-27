using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Features.Transport.Interfaces;
using SchoolERP.Application.Features.Transport.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/transport")]
public sealed class TransportController(ITransportService transportService) : ControllerBase
{
    [HttpPost("vehicles")]
    [ModuleAccess(ModuleCodes.TransportManagement, PermissionActions.Create)]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await transportService.CreateVehicleAsync(request, cancellationToken), "Vehicle created successfully."));

    [HttpPost("routes")]
    [ModuleAccess(ModuleCodes.TransportManagement, PermissionActions.Create)]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await transportService.CreateRouteAsync(request, cancellationToken), "Route created successfully."));

    [HttpPost("drivers")]
    [ModuleAccess(ModuleCodes.TransportManagement, PermissionActions.Create)]
    public async Task<IActionResult> CreateDriver([FromBody] CreateDriverRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await transportService.CreateDriverAsync(request, cancellationToken), "Driver created successfully."));

    [HttpPost("assignments")]
    [ModuleAccess(ModuleCodes.TransportManagement, PermissionActions.Create)]
    public async Task<IActionResult> AssignStudent([FromBody] AssignStudentTransportRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await transportService.AssignStudentAsync(request, cancellationToken), "Student transport assigned successfully."));

    [HttpGet("vehicles")]
    [ModuleAccess(ModuleCodes.TransportManagement, PermissionActions.View)]
    public async Task<IActionResult> GetVehicles([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await transportService.GetVehiclesAsync(schoolId, cancellationToken), "Vehicles fetched successfully."));

    [HttpGet("routes")]
    [ModuleAccess(ModuleCodes.TransportManagement, PermissionActions.View)]
    public async Task<IActionResult> GetRoutes([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await transportService.GetRoutesAsync(schoolId, cancellationToken), "Routes fetched successfully."));

    [HttpGet("drivers")]
    [ModuleAccess(ModuleCodes.TransportManagement, PermissionActions.View)]
    public async Task<IActionResult> GetDrivers([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await transportService.GetDriversAsync(schoolId, cancellationToken), "Drivers fetched successfully."));

    [HttpGet("assignments")]
    [ModuleAccess(ModuleCodes.TransportManagement, PermissionActions.View)]
    public async Task<IActionResult> GetAssignments([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await transportService.GetAssignmentsAsync(schoolId, cancellationToken), "Transport assignments fetched successfully."));
}
