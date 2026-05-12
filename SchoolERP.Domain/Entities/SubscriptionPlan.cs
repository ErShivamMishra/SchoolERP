using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class SubscriptionPlan : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PlanModuleEntitlement> PlanModuleEntitlements { get; set; } = new List<PlanModuleEntitlement>();
    public ICollection<SchoolSubscription> SchoolSubscriptions { get; set; } = new List<SchoolSubscription>();
}
