using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Features.AdmitCards.Interfaces;
using SchoolERP.Application.Features.AdmitCards.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/admit-cards")]
public sealed class AdmitCardsController(IAdmitCardService admitCardService) : ControllerBase
{
    [HttpPost("templates")]
    [ModuleAccess(ModuleCodes.AdmitCardManagement, PermissionActions.Create)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateAdmitCardTemplateRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await admitCardService.CreateTemplateAsync(request, cancellationToken), "Admit card template created successfully."));

    [HttpPost("generate")]
    [ModuleAccess(ModuleCodes.AdmitCardManagement, PermissionActions.Create)]
    public async Task<IActionResult> Generate([FromBody] GenerateAdmitCardsRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await admitCardService.GenerateAsync(request, cancellationToken), "Admit cards generated successfully."));
}
