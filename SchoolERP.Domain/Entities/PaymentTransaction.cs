using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class PaymentTransaction : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }

    public School? School { get; set; }
    public Invoice? Invoice { get; set; }
}
