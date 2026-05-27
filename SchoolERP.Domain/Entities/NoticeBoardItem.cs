using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class NoticeBoardItem : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NoticeType NoticeType { get; set; }
    public NoticeAudienceType AudienceType { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ExpiryDateUtc { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? OriginalFileName { get; set; }
    public string? StoredFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
}
