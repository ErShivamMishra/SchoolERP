namespace SchoolERP.Domain.Entities;

public sealed class PlanModuleEntitlement
{
    public Guid SubscriptionPlanId { get; set; }
    public Guid ModuleId { get; set; }
    public bool IsLicensed { get; set; }
    public bool IsVisibleInMenu { get; set; } = true;

    public SubscriptionPlan? SubscriptionPlan { get; set; }
    public Module? Module { get; set; }
}
