using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class FeeInstallment : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid InvoiceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }

    public School? School { get; set; }
    public Invoice? Invoice { get; set; }
}
