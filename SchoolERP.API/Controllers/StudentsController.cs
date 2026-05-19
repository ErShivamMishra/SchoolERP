using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Requests;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Students.Interfaces;
using SchoolERP.Application.Features.Students.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/students")]
public sealed class StudentsController(IStudentService studentService) : ControllerBase
{
    [HttpPost("from-admission")]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConvertAdmission([FromBody] ConvertAdmissionToStudentRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studentService.ConvertAdmissionAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Admission converted to student successfully."));
    }

    [HttpPost]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studentService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Student created successfully."));
    }

    [HttpPut("{studentId:guid}")]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid studentId, [FromBody] UpdateStudentRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studentService.UpdateAsync(studentId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Student updated successfully."));
    }

    [HttpGet("{studentId:guid}")]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid studentId, CancellationToken cancellationToken)
    {
        var response = await studentService.GetByIdAsync(studentId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Student fetched successfully."));
    }

    [HttpGet]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StudentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetStudentsRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studentService.GetAllAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Students fetched successfully."));
    }

    [HttpPatch("{studentId:guid}/promote")]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Promote(Guid studentId, [FromBody] PromoteStudentRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studentService.PromoteAsync(studentId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Student promoted successfully."));
    }

    [HttpPatch("{studentId:guid}/transfer")]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Transfer(Guid studentId, [FromBody] TransferStudentRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studentService.TransferAsync(studentId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Student transferred successfully."));
    }

    [HttpPatch("{studentId:guid}/deactivate")]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(Guid studentId, [FromBody] DeactivateStudentRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studentService.DeactivateAsync(studentId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Student deactivated successfully."));
    }

    [HttpPost("{studentId:guid}/documents")]
    [ModuleAccess(ModuleCodes.StudentManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<StudentDocumentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadDocument(Guid studentId, [FromForm] UploadStudentDocumentFormRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studentService.UploadDocumentAsync(studentId, new UploadStudentDocumentRequestDto
        {
            Title = request.Title,
            File = await ToPayloadAsync(request.File, cancellationToken)
        }, cancellationToken);

        return Ok(ApiResponseFactory.Success(response, "Student document uploaded successfully."));
    }

    private static async Task<FileUploadPayload?> ToPayloadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return null;
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        return new FileUploadPayload
        {
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            Content = memory.ToArray()
        };
    }
}
