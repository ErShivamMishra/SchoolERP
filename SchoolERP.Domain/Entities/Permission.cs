using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Permission : AuditableEntity
{
    public Guid ModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public Module? Module { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
