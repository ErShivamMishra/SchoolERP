using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Syllabus : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }
    public Guid AcademicSessionId { get; set; }
    public string Topics { get; set; } = string.Empty;
    public string? Description { get; set; }

    public School? School { get; set; }
    public Subject? Subject { get; set; }
    public Class? Class { get; set; }
    public AcademicSession? AcademicSession { get; set; }
}
