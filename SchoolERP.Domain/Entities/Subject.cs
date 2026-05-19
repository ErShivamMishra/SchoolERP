using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Subject : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public School? School { get; set; }
    public ICollection<Syllabus> Syllabi { get; set; } = new List<Syllabus>();
    public ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    public ICollection<StudyMaterial> StudyMaterials { get; set; } = new List<StudyMaterial>();
    public ICollection<HomeworkAssignment> HomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
}
