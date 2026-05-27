using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Parent : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Occupation { get; set; }
    public bool IsActive { get; set; } = true;

    public School? School { get; set; }
    public User? User { get; set; }
    public ICollection<ParentStudentRelation> StudentRelations { get; set; } = new List<ParentStudentRelation>();
}
