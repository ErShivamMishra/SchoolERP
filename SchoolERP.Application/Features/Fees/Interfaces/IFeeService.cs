using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Fees.Models;

namespace SchoolERP.Application.Features.Fees.Interfaces;

public interface IFeeService
{
    Task<FeeCategoryDto> CreateCategoryAsync(CreateFeeCategoryRequestDto request, CancellationToken cancellationToken);
    Task<FeeStructureDto> CreateStructureAsync(CreateFeeStructureRequestDto request, CancellationToken cancellationToken);
    Task<FineRuleDto> CreateFineRuleAsync(CreateFineRuleRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StudentFeeAssignmentDto>> AssignFeesAsync(AssignStudentFeesRequestDto request, CancellationToken cancellationToken);
    Task<InvoiceDto> GenerateInvoiceAsync(GenerateInvoiceRequestDto request, CancellationToken cancellationToken);
    Task<InvoiceDto> RecordPaymentAsync(Guid invoiceId, RecordPaymentRequestDto request, CancellationToken cancellationToken);
    Task<PagedResult<InvoiceDto>> GetInvoicesAsync(FeeInvoiceListRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PaymentTransactionDto>> GetPaymentHistoryAsync(Guid studentId, Guid? schoolId, CancellationToken cancellationToken);
    Task<FeeAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<PagedResult<FeeExportRowDto>> ExportInvoicesAsync(FeeInvoiceListRequestDto request, CancellationToken cancellationToken);
}
