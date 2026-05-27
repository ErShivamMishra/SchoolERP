using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class ParentStudentRelation : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }
    public string RelationshipType { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
    public bool CanViewAttendance { get; set; } = true;
    public bool CanViewFees { get; set; } = true;
    public bool CanViewResults { get; set; } = true;
    public bool CanViewHomework { get; set; } = true;
    public bool CanViewNotices { get; set; } = true;

    public School? School { get; set; }
    public Parent? Parent { get; set; }
    public Student? Student { get; set; }
}
