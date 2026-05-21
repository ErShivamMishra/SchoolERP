using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Fees.Models;
using SchoolERP.Application.Features.Reports.Interfaces;
using SchoolERP.Application.Features.Reports.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("exports/students")]
    [ModuleAccess(ModuleCodes.DashboardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StudentExportRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportStudents([FromQuery] StudentExportRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await reportService.ExportStudentsAsync(request, cancellationToken), "Student export prepared successfully."));

    [HttpGet("exports/attendance")]
    [ModuleAccess(ModuleCodes.DashboardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceExportRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAttendance([FromQuery] AttendanceExportRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await reportService.ExportAttendanceAsync(request, cancellationToken), "Attendance export prepared successfully."));

    [HttpGet("exports/fees")]
    [ModuleAccess(ModuleCodes.DashboardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FeeExportRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportFees([FromQuery] FeeInvoiceListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await reportService.ExportFeesAsync(request, cancellationToken), "Fee export prepared successfully."));

    [HttpGet("exports/quiz-results")]
    [ModuleAccess(ModuleCodes.DashboardManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<QuizResultExportRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportQuizResults([FromQuery] QuizResultExportRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await reportService.ExportQuizResultsAsync(request, cancellationToken), "Quiz result export prepared successfully."));
}
