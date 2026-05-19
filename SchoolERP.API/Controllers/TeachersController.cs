using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Teachers.Interfaces;
using SchoolERP.Application.Features.Teachers.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/teachers")]
public sealed class TeachersController(ITeacherService teacherService) : ControllerBase
{
    [HttpPost]
    [ModuleAccess(ModuleCodes.TeacherManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateTeacherRequestDto request, CancellationToken cancellationToken)
    {
        var response = await teacherService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Teacher created successfully."));
    }

    [HttpPut("{teacherId:guid}")]
    [ModuleAccess(ModuleCodes.TeacherManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid teacherId, [FromBody] UpdateTeacherRequestDto request, CancellationToken cancellationToken)
    {
        var response = await teacherService.UpdateAsync(teacherId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Teacher updated successfully."));
    }

    [HttpPut("{teacherId:guid}/subjects")]
    [ModuleAccess(ModuleCodes.TeacherManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignSubjects(Guid teacherId, [FromBody] AssignTeacherSubjectsRequestDto request, CancellationToken cancellationToken)
    {
        var response = await teacherService.AssignSubjectsAsync(teacherId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Teacher subjects assigned successfully."));
    }

    [HttpPut("{teacherId:guid}/classes")]
    [ModuleAccess(ModuleCodes.TeacherManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignClasses(Guid teacherId, [FromBody] AssignTeacherClassesRequestDto request, CancellationToken cancellationToken)
    {
        var response = await teacherService.AssignClassesAsync(teacherId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Teacher classes assigned successfully."));
    }

    [HttpGet("{teacherId:guid}")]
    [ModuleAccess(ModuleCodes.TeacherManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid teacherId, CancellationToken cancellationToken)
    {
        var response = await teacherService.GetByIdAsync(teacherId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Teacher fetched successfully."));
    }

    [HttpGet]
    [ModuleAccess(ModuleCodes.TeacherManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TeacherDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetTeachersRequestDto request, CancellationToken cancellationToken)
    {
        var response = await teacherService.GetAllAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Teachers fetched successfully."));
    }

    [HttpPatch("{teacherId:guid}/deactivate")]
    [ModuleAccess(ModuleCodes.TeacherManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<TeacherDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid teacherId, [FromBody] DeactivateTeacherRequestDto request, CancellationToken cancellationToken)
    {
        var response = await teacherService.DeactivateAsync(teacherId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Teacher deactivated successfully."));
    }
}
