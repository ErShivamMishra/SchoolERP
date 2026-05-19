using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class Student : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid? AdmissionId { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirthUtc { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Address { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid AcademicSessionId { get; set; }
    public DateTime AdmissionDateUtc { get; set; }
    public string? BloodGroup { get; set; }
    public string? Religion { get; set; }
    public string? Category { get; set; }
    public string? AadhaarNumber { get; set; }
    public bool IsActive { get; set; } = true;

    public School? School { get; set; }
    public Admission? Admission { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
    public AcademicSession? AcademicSession { get; set; }
    public StudentAcademicInfo? AcademicInfo { get; set; }
    public StudentAttendanceSummary? AttendanceSummary { get; set; }
    public ICollection<StudentDocument> Documents { get; set; } = new List<StudentDocument>();
}
