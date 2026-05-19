using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class Admission : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public string StudentFirstName { get; set; } = string.Empty;
    public string StudentLastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirthUtc { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? PreviousSchool { get; set; }
    public string GuardianName { get; set; } = string.Empty;
    public string GuardianPhone { get; set; } = string.Empty;
    public string GuardianRelation { get; set; } = string.Empty;
    public Guid AppliedClassId { get; set; }
    public Guid AcademicSessionId { get; set; }
    public DateTime AdmissionDateUtc { get; set; }
    public AdmissionStatus Status { get; set; } = AdmissionStatus.Pending;
    public string? Remarks { get; set; }

    public School? School { get; set; }
    public Class? AppliedClass { get; set; }
    public AcademicSession? AcademicSession { get; set; }
    public GuardianDetails? GuardianDetails { get; set; }
    public Student? Student { get; set; }
}
