using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class TeacherSubject : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }

    public School? School { get; set; }
    public Teacher? Teacher { get; set; }
    public Subject? Subject { get; set; }
}
