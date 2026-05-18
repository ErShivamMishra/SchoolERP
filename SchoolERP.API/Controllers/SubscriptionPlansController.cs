using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Features.Subscriptions.Interfaces;
using SchoolERP.Application.Features.Subscriptions.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/v1/subscription-plans")]
public sealed class SubscriptionPlansController(ISubscriptionPlanService subscriptionPlanService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanRequestDto request, CancellationToken cancellationToken)
    {
        var response = await subscriptionPlanService.CreatePlanAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Subscription plan created successfully."));
    }

    [HttpPut("{planId:guid}/modules")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PlanModuleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignModules(Guid planId, [FromBody] AssignPlanModulesRequestDto request, CancellationToken cancellationToken)
    {
        var response = await subscriptionPlanService.AssignModulesAsync(planId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Plan modules updated successfully."));
    }

    [HttpPost("/api/v1/schools/{schoolId:guid}/subscription-plan")]
    [ProducesResponseType(typeof(ApiResponse<SchoolSubscriptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignPlanToSchool(Guid schoolId, [FromBody] AssignSchoolPlanRequestDto request, CancellationToken cancellationToken)
    {
        var response = await subscriptionPlanService.AssignPlanToSchoolAsync(schoolId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "School subscription plan assigned successfully."));
    }
}
