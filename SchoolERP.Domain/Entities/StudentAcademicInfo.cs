using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class StudentAcademicInfo : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public string? PreviousSchool { get; set; }
    public string? Remarks { get; set; }

    public School? School { get; set; }
    public Student? Student { get; set; }
}
