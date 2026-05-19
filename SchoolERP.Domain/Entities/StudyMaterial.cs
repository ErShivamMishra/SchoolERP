using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class StudyMaterial : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDateUtc { get; set; }

    public School? School { get; set; }
    public Subject? Subject { get; set; }
    public Teacher? Teacher { get; set; }
}
