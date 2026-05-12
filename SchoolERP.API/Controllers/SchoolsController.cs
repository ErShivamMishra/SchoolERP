using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Application.Features.Schools.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/v1/schools")]
public sealed class SchoolsController(ISchoolService schoolService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateSchoolResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateSchoolRequestDto request, CancellationToken cancellationToken)
    {
        var response = await schoolService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "School created successfully."));
    }

    [HttpPut("{schoolId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SchoolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid schoolId, [FromBody] UpdateSchoolRequestDto request, CancellationToken cancellationToken)
    {
        var response = await schoolService.UpdateAsync(schoolId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "School updated successfully."));
    }

    [HttpGet("{schoolId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SchoolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid schoolId, CancellationToken cancellationToken)
    {
        var response = await schoolService.GetByIdAsync(schoolId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "School fetched successfully."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SchoolDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await schoolService.GetAllAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Schools fetched successfully."));
    }

    [HttpPatch("{schoolId:guid}/activation")]
    [ProducesResponseType(typeof(ApiResponse<SchoolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetActivation(Guid schoolId, [FromBody] SetSchoolActivationRequestDto request, CancellationToken cancellationToken)
    {
        var response = await schoolService.SetActivationAsync(schoolId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "School activation updated successfully."));
    }

    [HttpPatch("{schoolId:guid}/subscription/extend")]
    [ProducesResponseType(typeof(ApiResponse<SchoolDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExtendSubscription(Guid schoolId, [FromBody] ExtendSchoolSubscriptionRequestDto request, CancellationToken cancellationToken)
    {
        var response = await schoolService.ExtendSubscriptionAsync(schoolId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "School subscription extended successfully."));
    }
}
