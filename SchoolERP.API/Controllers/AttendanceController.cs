using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Attendance.Interfaces;
using SchoolERP.Application.Features.Attendance.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/attendance")]
public sealed class AttendanceController(IAttendanceService attendanceService) : ControllerBase
{
    [HttpPost("mark")]
    [ModuleAccess(ModuleCodes.AttendanceManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mark([FromBody] MarkAttendanceRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await attendanceService.MarkAttendanceAsync(request, cancellationToken), "Attendance marked successfully."));

    [HttpPost("bulk")]
    [ModuleAccess(ModuleCodes.AttendanceManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceSessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Bulk([FromBody] MarkAttendanceRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await attendanceService.BulkMarkAttendanceAsync(request, cancellationToken), "Bulk attendance marked successfully."));

    [HttpPut("records/{attendanceRecordId:guid}")]
    [ModuleAccess(ModuleCodes.AttendanceManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid attendanceRecordId, [FromBody] UpdateAttendanceRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await attendanceService.UpdateAttendanceAsync(attendanceRecordId, request, cancellationToken), "Attendance updated successfully."));

    [HttpGet("daily-report")]
    [ModuleAccess(ModuleCodes.AttendanceManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceSessionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DailyReport([FromQuery] DailyAttendanceReportRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await attendanceService.GetDailyReportAsync(request, cancellationToken), "Attendance report fetched successfully."));

    [HttpGet("students/{studentId:guid}/history")]
    [ModuleAccess(ModuleCodes.AttendanceManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceRecordDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> StudentHistory(Guid studentId, [FromQuery] StudentAttendanceHistoryRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await attendanceService.GetStudentHistoryAsync(studentId, request, cancellationToken), "Attendance history fetched successfully."));

    [HttpGet("monthly-summary")]
    [ModuleAccess(ModuleCodes.AttendanceManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<AttendanceSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MonthlySummary([FromQuery] MonthlyAttendanceSummaryRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await attendanceService.GetMonthlySummaryAsync(request, cancellationToken), "Attendance summary fetched successfully."));

    [HttpGet("analytics")]
    [ModuleAccess(ModuleCodes.AttendanceManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Analytics([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await attendanceService.GetAnalyticsAsync(schoolId, cancellationToken), "Attendance analytics fetched successfully."));
}
