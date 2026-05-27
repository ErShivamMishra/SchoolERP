using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Features.IdCards.Interfaces;
using SchoolERP.Application.Features.IdCards.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/id-cards")]
public sealed class IdCardsController(IIdCardService idCardService) : ControllerBase
{
    [HttpPost("templates")]
    [ModuleAccess(ModuleCodes.IdCardManagement, PermissionActions.Create)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateIdCardTemplateRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await idCardService.CreateTemplateAsync(request, cancellationToken), "ID card template created successfully."));

    [HttpPost("generate/students")]
    [ModuleAccess(ModuleCodes.IdCardManagement, PermissionActions.Create)]
    public async Task<IActionResult> GenerateStudentCards([FromBody] GenerateStudentIdCardsRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await idCardService.GenerateStudentCardsAsync(request, cancellationToken), "Student ID cards generated successfully."));

    [HttpPost("generate/teachers")]
    [ModuleAccess(ModuleCodes.IdCardManagement, PermissionActions.Create)]
    public async Task<IActionResult> GenerateTeacherCards([FromBody] GenerateTeacherIdCardsRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await idCardService.GenerateTeacherCardsAsync(request, cancellationToken), "Teacher ID cards generated successfully."));
}
