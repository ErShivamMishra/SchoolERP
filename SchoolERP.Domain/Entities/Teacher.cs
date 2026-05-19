using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class Teacher : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Qualification { get; set; }
    public int ExperienceYears { get; set; }
    public DateTime JoiningDateUtc { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public School? School { get; set; }
    public ICollection<TeacherSubject> Subjects { get; set; } = new List<TeacherSubject>();
    public ICollection<TeacherClassAssignment> ClassAssignments { get; set; } = new List<TeacherClassAssignment>();
    public ICollection<StudyMaterial> StudyMaterials { get; set; } = new List<StudyMaterial>();
    public ICollection<HomeworkAssignment> HomeworkAssignments { get; set; } = new List<HomeworkAssignment>();
}
