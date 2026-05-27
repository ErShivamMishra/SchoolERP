using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Results.Interfaces;
using SchoolERP.Application.Features.Results.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/results")]
public sealed class ResultsController(IResultService resultService) : ControllerBase
{
    [HttpPost("exams")]
    [ModuleAccess(ModuleCodes.ResultManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await resultService.CreateExamAsync(request, cancellationToken), "Exam created successfully."));

    [HttpPatch("exams/{examId:guid}/publish")]
    [ModuleAccess(ModuleCodes.ResultManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PublishExam(Guid examId, [FromBody] PublishExamRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await resultService.PublishExamAsync(examId, request, cancellationToken), "Exam publication status updated successfully."));

    [HttpPost("exams/{examId:guid}/records")]
    [ModuleAccess(ModuleCodes.ResultManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ExamResultDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordResults(Guid examId, [FromBody] RecordExamResultsRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await resultService.RecordResultsAsync(examId, request, cancellationToken), "Exam results recorded successfully."));

    [HttpGet("exams")]
    [ModuleAccess(ModuleCodes.ResultManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ExamDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExams([FromQuery] GetExamListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await resultService.GetExamsAsync(request, cancellationToken), "Exams fetched successfully."));

    [HttpGet("exams/{examId:guid}")]
    [ModuleAccess(ModuleCodes.ResultManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExam(Guid examId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await resultService.GetExamByIdAsync(examId, cancellationToken), "Exam fetched successfully."));

    [HttpGet("exams/{examId:guid}/students/{studentId:guid}")]
    [ModuleAccess(ModuleCodes.ResultManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<StudentExamReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentReport(Guid examId, Guid studentId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await resultService.GetStudentReportAsync(examId, studentId, cancellationToken), "Student exam report fetched successfully."));

    [HttpGet("exams/{examId:guid}/class-results")]
    [ModuleAccess(ModuleCodes.ResultManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<ExamResultDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClassResults(Guid examId, [FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await resultService.GetClassResultsAsync(examId, schoolId, cancellationToken), "Class results fetched successfully."));

    [HttpGet("analytics")]
    [ModuleAccess(ModuleCodes.ResultManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<ResultAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalytics([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await resultService.GetAnalyticsAsync(schoolId, cancellationToken), "Result analytics fetched successfully."));
}
