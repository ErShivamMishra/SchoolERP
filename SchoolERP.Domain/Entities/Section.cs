using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Section : AuditableEntity
{
    public Guid TenantId { get; set; }
    public Guid ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public School? Tenant { get; set; }
    public Class? Class { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<TeacherClassAssignment> TeacherClassAssignments { get; set; } = new List<TeacherClassAssignment>();
    public ICollection<HomeworkAssignment> HomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
}
