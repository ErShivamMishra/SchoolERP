using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class AttendanceRecord : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid AttendanceSessionId { get; set; }
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public Guid MarkedByUserId { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public AttendanceSession? AttendanceSession { get; set; }
    public Student? Student { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
    public User? MarkedByUser { get; set; }
}
