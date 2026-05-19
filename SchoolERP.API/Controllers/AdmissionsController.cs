using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Admissions.Interfaces;
using SchoolERP.Application.Features.Admissions.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/admissions")]
public sealed class AdmissionsController(IAdmissionService admissionService) : ControllerBase
{
    [HttpPost("academic-sessions")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<AcademicSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAcademicSession([FromBody] CreateAcademicSessionRequestDto request, CancellationToken cancellationToken)
    {
        var response = await admissionService.CreateAcademicSessionAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Academic session created successfully."));
    }

    [HttpGet("academic-sessions")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<AcademicSessionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAcademicSessions([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
    {
        var response = await admissionService.GetAcademicSessionsAsync(schoolId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Academic sessions fetched successfully."));
    }

    [HttpPost("classes")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassRequestDto request, CancellationToken cancellationToken)
    {
        var response = await admissionService.CreateClassAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Class created successfully."));
    }

    [HttpGet("classes")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ClassDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClasses([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
    {
        var response = await admissionService.GetClassesAsync(schoolId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Classes fetched successfully."));
    }

    [HttpPost("sections")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionRequestDto request, CancellationToken cancellationToken)
    {
        var response = await admissionService.CreateSectionAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Section created successfully."));
    }

    [HttpGet("sections")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SectionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSections([FromQuery] Guid? schoolId, [FromQuery] Guid? classId, CancellationToken cancellationToken)
    {
        var response = await admissionService.GetSectionsAsync(schoolId, classId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Sections fetched successfully."));
    }

    [HttpPost]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<AdmissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateAdmissionRequestDto request, CancellationToken cancellationToken)
    {
        var response = await admissionService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Admission created successfully."));
    }

    [HttpPut("{admissionId:guid}")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<AdmissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid admissionId, [FromBody] UpdateAdmissionRequestDto request, CancellationToken cancellationToken)
    {
        var response = await admissionService.UpdateAsync(admissionId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Admission updated successfully."));
    }

    [HttpPatch("{admissionId:guid}/approve")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<AdmissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid admissionId, [FromBody] ChangeAdmissionStatusRequestDto request, CancellationToken cancellationToken)
    {
        var response = await admissionService.ApproveAsync(admissionId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Admission approved successfully."));
    }

    [HttpPatch("{admissionId:guid}/reject")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<AdmissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(Guid admissionId, [FromBody] ChangeAdmissionStatusRequestDto request, CancellationToken cancellationToken)
    {
        var response = await admissionService.RejectAsync(admissionId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Admission rejected successfully."));
    }

    [HttpGet("{admissionId:guid}")]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<AdmissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid admissionId, CancellationToken cancellationToken)
    {
        var response = await admissionService.GetByIdAsync(admissionId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Admission fetched successfully."));
    }

    [HttpGet]
    [ModuleAccess(ModuleCodes.AdmissionManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AdmissionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAdmissionsRequestDto request, CancellationToken cancellationToken)
    {
        var response = await admissionService.GetAllAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Admissions fetched successfully."));
    }
}
