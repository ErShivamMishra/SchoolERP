using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Class : AuditableEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public School? Tenant { get; set; }
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<TeacherClassAssignment> TeacherClassAssignments { get; set; } = new List<TeacherClassAssignment>();
    public ICollection<Syllabus> Syllabi { get; set; } = new List<Syllabus>();
    public ICollection<HomeworkAssignment> HomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
}
