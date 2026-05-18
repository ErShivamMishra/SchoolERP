using SchoolERP.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Domain.Entities;

public sealed class User : AuditableEntity
{
    public Guid? TenantId { get; set; }
    [NotMapped]
    public Guid? SchoolId
    {
        get => TenantId;
        set => TenantId = value;
    }

    public Guid? CampusId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPlatformUser { get; set; }
    public bool RequiresPasswordReset { get; set; } = true;
    [NotMapped]
    public bool IsFirstLogin
    {
        get => RequiresPasswordReset;
        set => RequiresPasswordReset = value;
    }

    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    [NotMapped]
    public DateTime? LockoutEnd
    {
        get => LockoutEndUtc;
        set => LockoutEndUtc = value;
    }

    public DateTime? LastLoginAtUtc { get; set; }

    public School? Tenant { get; set; }
    public Campus? Campus { get; set; }
    public Role? Role { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
