using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Module : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<PlanModuleEntitlement> PlanModuleEntitlements { get; set; } = new List<PlanModuleEntitlement>();
}
