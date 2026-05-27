using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Features.ParentPortal.Interfaces;
using SchoolERP.Application.Features.ParentPortal.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/v1/parents")]
public sealed class ParentsController(IParentPortalService parentPortalService) : ControllerBase
{
    [Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
    [HttpPost]
    [ModuleAccess(ModuleCodes.ParentPortalManagement, PermissionActions.Create)]
    public async Task<IActionResult> Create([FromBody] CreateParentRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await parentPortalService.CreateParentAsync(request, cancellationToken), "Parent account created successfully."));

    [Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
    [HttpPost("{parentId:guid}/students")]
    [ModuleAccess(ModuleCodes.ParentPortalManagement, PermissionActions.Edit)]
    public async Task<IActionResult> LinkStudent(Guid parentId, [FromBody] LinkParentStudentRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await parentPortalService.LinkStudentAsync(parentId, request, cancellationToken), "Parent linked to student successfully."));

    [Authorize(Roles = RoleNames.Parent)]
    [HttpGet("me/students")]
    [ModuleAccess(ModuleCodes.ParentPortalManagement, PermissionActions.View)]
    public async Task<IActionResult> GetMyStudents(CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await parentPortalService.GetMyStudentsAsync(cancellationToken), "Linked students fetched successfully."));

    [Authorize(Roles = RoleNames.Parent)]
    [HttpGet("me/students/{studentId:guid}/attendance")]
    [ModuleAccess(ModuleCodes.ParentPortalManagement, PermissionActions.View)]
    public async Task<IActionResult> GetMyStudentAttendance(Guid studentId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await parentPortalService.GetMyStudentAttendanceAsync(studentId, cancellationToken), "Student attendance fetched successfully."));

    [Authorize(Roles = RoleNames.Parent)]
    [HttpGet("me/students/{studentId:guid}/fees")]
    [ModuleAccess(ModuleCodes.ParentPortalManagement, PermissionActions.View)]
    public async Task<IActionResult> GetMyStudentFees(Guid studentId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await parentPortalService.GetMyStudentFeeStatusAsync(studentId, cancellationToken), "Student fee status fetched successfully."));

    [Authorize(Roles = RoleNames.Parent)]
    [HttpGet("me/students/{studentId:guid}/results")]
    [ModuleAccess(ModuleCodes.ParentPortalManagement, PermissionActions.View)]
    public async Task<IActionResult> GetMyStudentResults(Guid studentId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await parentPortalService.GetMyStudentResultsAsync(studentId, cancellationToken), "Student results fetched successfully."));

    [Authorize(Roles = RoleNames.Parent)]
    [HttpGet("me/students/{studentId:guid}/homework")]
    [ModuleAccess(ModuleCodes.ParentPortalManagement, PermissionActions.View)]
    public async Task<IActionResult> GetMyStudentHomework(Guid studentId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await parentPortalService.GetMyStudentHomeworkAsync(studentId, cancellationToken), "Student homework fetched successfully."));

    [Authorize(Roles = RoleNames.Parent)]
    [HttpGet("me/notices")]
    [ModuleAccess(ModuleCodes.ParentPortalManagement, PermissionActions.View)]
    public async Task<IActionResult> GetMyNotices(CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await parentPortalService.GetMyNoticesAsync(cancellationToken), "Parent notices fetched successfully."));
}
