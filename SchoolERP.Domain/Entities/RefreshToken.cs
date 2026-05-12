using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class RefreshToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
    public User? User { get; set; }
}
