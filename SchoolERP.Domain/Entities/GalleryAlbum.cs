using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class GalleryAlbum : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public ICollection<GalleryMedia> MediaItems { get; set; } = new List<GalleryMedia>();
}
