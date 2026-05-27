using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class IdCardTemplate : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string SchoolDetails { get; set; } = string.Empty;
    public string? LayoutJson { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public ICollection<GeneratedIdCard> GeneratedCards { get; set; } = new List<GeneratedIdCard>();
}
