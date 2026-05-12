using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class SchoolSubscription : AuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public DateTime? GracePeriodEndDateUtc { get; set; }

    public School? Tenant { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }
}
