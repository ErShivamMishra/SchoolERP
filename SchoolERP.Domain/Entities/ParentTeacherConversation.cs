using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class ParentTeacherConversation : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public Guid TeacherId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime LastMessageAtUtc { get; set; }
    public bool IsClosed { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
    public ICollection<ParentTeacherMessage> Messages { get; set; } = new List<ParentTeacherMessage>();
}
