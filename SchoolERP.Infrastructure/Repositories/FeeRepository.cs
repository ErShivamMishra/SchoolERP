using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Fees.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class FeeRepository(SchoolErpDbContext dbContext) : IFeeRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<FeeCategory?> GetFeeCategoryByIdAsync(Guid feeCategoryId, CancellationToken cancellationToken)
        => dbContext.FeeCategories.FirstOrDefaultAsync(x => x.Id == feeCategoryId, cancellationToken);

    public Task<FeeStructure?> GetFeeStructureByIdAsync(Guid feeStructureId, CancellationToken cancellationToken)
        => dbContext.FeeStructures
            .Include(x => x.FeeCategory)
            .FirstOrDefaultAsync(x => x.Id == feeStructureId, cancellationToken);

    public Task<FineRule?> GetFineRuleByIdAsync(Guid fineRuleId, CancellationToken cancellationToken)
        => dbContext.FineRules.FirstOrDefaultAsync(x => x.Id == fineRuleId, cancellationToken);

    public Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken)
        => dbContext.Students.FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

    public async Task<IReadOnlyCollection<Student>> GetStudentsForAssignmentAsync(Guid schoolId, Guid? classId, Guid? sectionId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken)
    {
        var query = dbContext.Students.Where(x => x.SchoolId == schoolId && x.IsActive);

        if (studentIds.Count > 0)
        {
            query = query.Where(x => studentIds.Contains(x.Id));
        }
        else if (classId.HasValue)
        {
            query = query.Where(x => x.ClassId == classId.Value);
            if (sectionId.HasValue)
            {
                query = query.Where(x => x.SectionId == sectionId.Value);
            }
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<StudentFeeAssignment?> GetStudentFeeAssignmentByIdAsync(Guid assignmentId, CancellationToken cancellationToken)
        => dbContext.StudentFeeAssignments.FirstOrDefaultAsync(x => x.Id == assignmentId, cancellationToken);

    public Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken)
        => dbContext.Invoices
            .Include(x => x.Student)
            .Include(x => x.Installments)
            .Include(x => x.PaymentTransactions)
            .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);

    public async Task<string> GenerateInvoiceNumberAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var count = await dbContext.Invoices.CountAsync(x => x.SchoolId == schoolId, cancellationToken);
        return $"INV-{DateTime.UtcNow:yyyyMMdd}-{count + 1:D5}";
    }

    public async Task<(IReadOnlyCollection<Invoice> Items, int TotalCount)> GetInvoicesPageAsync(Guid schoolId, Guid? studentId, InvoiceStatus? status, DateTime? fromDate, DateTime? toDate, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Invoices
            .Include(x => x.Student)
            .Include(x => x.Installments)
            .Include(x => x.PaymentTransactions)
            .Where(x => x.SchoolId == schoolId);

        if (studentId.HasValue)
        {
            query = query.Where(x => x.StudentId == studentId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.InvoiceDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.InvoiceDate <= toDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.InvoiceNumber.Contains(term) ||
                x.Student!.FirstName.Contains(term) ||
                x.Student.LastName.Contains(term) ||
                x.Student.RollNumber.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.InvoiceDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyCollection<PaymentTransaction>> GetStudentPaymentsAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken)
        => await dbContext.PaymentTransactions
            .Include(x => x.Invoice)
            .Where(x => x.SchoolId == schoolId && x.Invoice!.StudentId == studentId)
            .OrderByDescending(x => x.PaymentDate)
            .ToListAsync(cancellationToken);

    public async Task<(decimal PendingFees, decimal CollectedAmount, decimal OverdueAmount, int OverdueInvoices, IReadOnlyCollection<(int Year, int Month, decimal Amount)> RevenueTrend)> GetAnalyticsAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var pendingFees = await dbContext.Invoices.Where(x => x.SchoolId == schoolId).SumAsync(x => x.PendingAmount, cancellationToken);
        var collectedAmount = await dbContext.PaymentTransactions.Where(x => x.SchoolId == schoolId).SumAsync(x => x.Amount, cancellationToken);
        var overdueInvoices = await dbContext.Invoices.CountAsync(x => x.SchoolId == schoolId && x.Status == InvoiceStatus.Overdue, cancellationToken);
        var overdueAmount = await dbContext.Invoices.Where(x => x.SchoolId == schoolId && x.Status == InvoiceStatus.Overdue).SumAsync(x => x.PendingAmount, cancellationToken);
        var revenueTrend = await dbContext.PaymentTransactions
            .Where(x => x.SchoolId == schoolId)
            .GroupBy(x => new { x.PaymentDate.Year, x.PaymentDate.Month })
            .Select(x => new ValueTuple<int, int, decimal>(x.Key.Year, x.Key.Month, x.Sum(y => y.Amount)))
            .ToListAsync(cancellationToken);

        return (pendingFees, collectedAmount, overdueAmount, overdueInvoices, revenueTrend);
    }

    public Task AddFeeCategoryAsync(FeeCategory category, CancellationToken cancellationToken)
        => dbContext.FeeCategories.AddAsync(category, cancellationToken).AsTask();

    public Task AddFeeStructureAsync(FeeStructure structure, CancellationToken cancellationToken)
        => dbContext.FeeStructures.AddAsync(structure, cancellationToken).AsTask();

    public Task AddFineRuleAsync(FineRule fineRule, CancellationToken cancellationToken)
        => dbContext.FineRules.AddAsync(fineRule, cancellationToken).AsTask();

    public Task AddStudentFeeAssignmentsAsync(IEnumerable<StudentFeeAssignment> assignments, CancellationToken cancellationToken)
        => dbContext.StudentFeeAssignments.AddRangeAsync(assignments, cancellationToken);

    public Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
        => dbContext.Invoices.AddAsync(invoice, cancellationToken).AsTask();

    public Task AddPaymentTransactionAsync(PaymentTransaction paymentTransaction, CancellationToken cancellationToken)
        => dbContext.PaymentTransactions.AddAsync(paymentTransaction, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
