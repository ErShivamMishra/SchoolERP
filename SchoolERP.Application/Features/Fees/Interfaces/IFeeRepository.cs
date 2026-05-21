using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Fees.Interfaces;

public interface IFeeRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<FeeCategory?> GetFeeCategoryByIdAsync(Guid feeCategoryId, CancellationToken cancellationToken);
    Task<FeeStructure?> GetFeeStructureByIdAsync(Guid feeStructureId, CancellationToken cancellationToken);
    Task<FineRule?> GetFineRuleByIdAsync(Guid fineRuleId, CancellationToken cancellationToken);
    Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Student>> GetStudentsForAssignmentAsync(Guid schoolId, Guid? classId, Guid? sectionId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken);
    Task<StudentFeeAssignment?> GetStudentFeeAssignmentByIdAsync(Guid assignmentId, CancellationToken cancellationToken);
    Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken);
    Task<string> GenerateInvoiceNumberAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Invoice> Items, int TotalCount)> GetInvoicesPageAsync(Guid schoolId, Guid? studentId, InvoiceStatus? status, DateTime? fromDate, DateTime? toDate, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PaymentTransaction>> GetStudentPaymentsAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken);
    Task<(decimal PendingFees, decimal CollectedAmount, decimal OverdueAmount, int OverdueInvoices, IReadOnlyCollection<(int Year, int Month, decimal Amount)> RevenueTrend)> GetAnalyticsAsync(Guid schoolId, CancellationToken cancellationToken);
    Task AddFeeCategoryAsync(FeeCategory category, CancellationToken cancellationToken);
    Task AddFeeStructureAsync(FeeStructure structure, CancellationToken cancellationToken);
    Task AddFineRuleAsync(FineRule fineRule, CancellationToken cancellationToken);
    Task AddStudentFeeAssignmentsAsync(IEnumerable<StudentFeeAssignment> assignments, CancellationToken cancellationToken);
    Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken);
    Task AddPaymentTransactionAsync(PaymentTransaction paymentTransaction, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
