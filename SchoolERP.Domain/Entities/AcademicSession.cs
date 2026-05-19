using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class AcademicSession : AuditableEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public School? Tenant { get; set; }
    public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Syllabus> Syllabi { get; set; } = new List<Syllabus>();
    public ICollection<TeacherClassAssignment> TeacherClassAssignments { get; set; } = new List<TeacherClassAssignment>();
}
