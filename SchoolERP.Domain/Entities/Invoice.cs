using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class Invoice : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? StudentFeeAssignmentId { get; set; }
    public Guid? FineRuleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal FineAmount { get; set; }
    public string? ReminderMetadata { get; set; }
    public InvoiceStatus Status { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Student? Student { get; set; }
    public StudentFeeAssignment? StudentFeeAssignment { get; set; }
    public FineRule? FineRule { get; set; }
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
    public ICollection<FeeInstallment> Installments { get; set; } = new List<FeeInstallment>();
}
