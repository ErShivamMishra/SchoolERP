using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Module : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<PlanModuleEntitlement> PlanModuleEntitlements { get; set; } = new List<PlanModuleEntitlement>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
