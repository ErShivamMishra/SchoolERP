using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Features.Modules.Interfaces;
using SchoolERP.Application.Features.Modules.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/v1/modules")]
public sealed class ModulesController(IModuleService moduleService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ModuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateModuleRequestDto request, CancellationToken cancellationToken)
    {
        var response = await moduleService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Module created successfully."));
    }

    [HttpPut("{moduleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ModuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid moduleId, [FromBody] UpdateModuleRequestDto request, CancellationToken cancellationToken)
    {
        var response = await moduleService.UpdateAsync(moduleId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Module updated successfully."));
    }

    [HttpPatch("{moduleId:guid}/activation")]
    [ProducesResponseType(typeof(ApiResponse<ModuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetActivation(Guid moduleId, [FromBody] SetModuleActivationRequestDto request, CancellationToken cancellationToken)
    {
        var response = await moduleService.SetActivationAsync(moduleId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Module activation updated successfully."));
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ModuleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await moduleService.GetAllAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Modules fetched successfully."));
    }
}
