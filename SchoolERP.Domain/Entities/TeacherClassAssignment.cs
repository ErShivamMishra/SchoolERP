using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class TeacherClassAssignment : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid AcademicSessionId { get; set; }

    public School? School { get; set; }
    public Teacher? Teacher { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
    public AcademicSession? AcademicSession { get; set; }
}
