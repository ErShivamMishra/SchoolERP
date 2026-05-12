using SchoolERP.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Domain.Entities;

public sealed class Role : AuditableEntity
{
    public Guid? TenantId { get; set; }
    [NotMapped]
    public Guid? SchoolId
    {
        get => TenantId;
        set => TenantId = value;
    }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
