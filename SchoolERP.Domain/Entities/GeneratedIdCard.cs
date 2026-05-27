using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class GeneratedIdCard : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid TemplateId { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? TeacherId { get; set; }
    public string CardHolderType { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string CardIdentifier { get; set; } = string.Empty;
    public string? QrCodePayload { get; set; }
    public string? BarcodePayload { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    public School? School { get; set; }
    public IdCardTemplate? Template { get; set; }
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
}
