using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
