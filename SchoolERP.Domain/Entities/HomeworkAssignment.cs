using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class HomeworkAssignment : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime DueDateUtc { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? OriginalFileName { get; set; }
    public string? StoredFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }

    public School? School { get; set; }
    public Subject? Subject { get; set; }
    public Teacher? Teacher { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
}
