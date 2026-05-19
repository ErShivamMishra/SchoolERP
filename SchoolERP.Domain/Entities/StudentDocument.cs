using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class StudentDocument : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;

    public School? School { get; set; }
    public Student? Student { get; set; }
}
