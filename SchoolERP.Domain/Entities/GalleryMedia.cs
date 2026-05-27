using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class GalleryMedia : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid AlbumId { get; set; }
    public MediaType MediaType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }

    public School? School { get; set; }
    public GalleryAlbum? Album { get; set; }
}
