using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Authorization;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Fees.Interfaces;
using SchoolERP.Application.Features.Fees.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.API.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.SuperAdmin},{RoleNames.SchoolAdmin},{RoleNames.Staff}")]
[Route("api/v1/fees")]
public sealed class FeesController(IFeeService feeService) : ControllerBase
{
    [HttpPost("categories")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<FeeCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateFeeCategoryRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.CreateCategoryAsync(request, cancellationToken), "Fee category created successfully."));

    [HttpPost("structures")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<FeeStructureDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateStructure([FromBody] CreateFeeStructureRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.CreateStructureAsync(request, cancellationToken), "Fee structure created successfully."));

    [HttpPost("fine-rules")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<FineRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateFineRule([FromBody] CreateFineRuleRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.CreateFineRuleAsync(request, cancellationToken), "Fine rule created successfully."));

    [HttpPost("assignments")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<StudentFeeAssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignFees([FromBody] AssignStudentFeesRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.AssignFeesAsync(request, cancellationToken), "Fees assigned successfully."));

    [HttpPost("invoices")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.Create)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateInvoice([FromBody] GenerateInvoiceRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.GenerateInvoiceAsync(request, cancellationToken), "Invoice generated successfully."));

    [HttpPost("invoices/{invoiceId:guid}/payments")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.Edit)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordPayment(Guid invoiceId, [FromBody] RecordPaymentRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.RecordPaymentAsync(invoiceId, request, cancellationToken), "Payment recorded successfully."));

    [HttpGet("invoices")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices([FromQuery] FeeInvoiceListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.GetInvoicesAsync(request, cancellationToken), "Invoices fetched successfully."));

    [HttpGet("students/{studentId:guid}/payments")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PaymentTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentHistory(Guid studentId, [FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.GetPaymentHistoryAsync(studentId, schoolId, cancellationToken), "Payment history fetched successfully."));

    [HttpGet("analytics")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<FeeAnalyticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Analytics([FromQuery] Guid? schoolId, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.GetAnalyticsAsync(schoolId, cancellationToken), "Fee analytics fetched successfully."));

    [HttpGet("exports/invoices")]
    [ModuleAccess(ModuleCodes.FeeManagement, PermissionActions.View)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FeeExportRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportInvoices([FromQuery] FeeInvoiceListRequestDto request, CancellationToken cancellationToken)
        => Ok(ApiResponseFactory.Success(await feeService.ExportInvoicesAsync(request, cancellationToken), "Invoice export prepared successfully."));
}
