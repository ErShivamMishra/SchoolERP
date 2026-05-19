using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Requests;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.FileStorage;
using SchoolERP.Application.Features.Study.Interfaces;
using SchoolERP.Application.Features.Study.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/study")]
public sealed class StudyController(IStudyService studyService) : ControllerBase
{
    [HttpPost("subjects")]
    [ModuleAccess(ModuleCodes.StudyManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studyService.CreateSubjectAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Subject created successfully."));
    }

    [HttpPut("subjects/{subjectId:guid}")]
    [ModuleAccess(ModuleCodes.StudyManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSubject(Guid subjectId, [FromBody] UpdateSubjectRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studyService.UpdateSubjectAsync(subjectId, request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Subject updated successfully."));
    }

    [HttpGet("subjects")]
    [ModuleAccess(ModuleCodes.StudyManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<SubjectDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubjects([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
    {
        var response = await studyService.GetSubjectsAsync(schoolId, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Subjects fetched successfully."));
    }

    [HttpPost("syllabi")]
    [ModuleAccess(ModuleCodes.StudyManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<SyllabusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadSyllabus([FromBody] UploadSyllabusRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studyService.UploadSyllabusAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Syllabus saved successfully."));
    }

    [HttpPost("materials")]
    [ModuleAccess(ModuleCodes.StudyManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<StudyMaterialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadStudyMaterial([FromForm] UploadStudyMaterialFormRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studyService.UploadStudyMaterialAsync(new UploadStudyMaterialRequestDto
        {
            SchoolId = request.SchoolId,
            SubjectId = request.SubjectId,
            TeacherId = request.TeacherId,
            Title = request.Title,
            Description = request.Description,
            File = await ToPayloadAsync(request.File, cancellationToken)
        }, cancellationToken);

        return Ok(ApiResponseFactory.Success(response, "Study material uploaded successfully."));
    }

    [HttpPost("homework")]
    [ModuleAccess(ModuleCodes.StudyManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<HomeworkAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateHomework([FromForm] CreateHomeworkAssignmentFormRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studyService.CreateHomeworkAsync(new CreateHomeworkAssignmentRequestDto
        {
            SchoolId = request.SchoolId,
            SubjectId = request.SubjectId,
            TeacherId = request.TeacherId,
            ClassId = request.ClassId,
            SectionId = request.SectionId,
            Title = request.Title,
            Instructions = request.Instructions,
            DueDate = request.DueDate,
            Attachment = await ToPayloadAsync(request.Attachment, cancellationToken)
        }, cancellationToken);

        return Ok(ApiResponseFactory.Success(response, "Homework created successfully."));
    }

    [HttpGet("materials")]
    [ModuleAccess(ModuleCodes.StudyManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<StudyMaterialDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudyMaterials([FromQuery] GetStudyMaterialsRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studyService.GetStudyMaterialsAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Study materials fetched successfully."));
    }

    [HttpGet("homework")]
    [ModuleAccess(ModuleCodes.StudyManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HomeworkAssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHomework([FromQuery] GetHomeworkAssignmentsRequestDto request, CancellationToken cancellationToken)
    {
        var response = await studyService.GetHomeworkAssignmentsAsync(request, cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Homework assignments fetched successfully."));
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
