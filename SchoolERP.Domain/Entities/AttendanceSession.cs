using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class AttendanceSession : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string? SessionName { get; set; }
    public Guid MarkedByUserId { get; set; }
    public int TotalStudents { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int HalfDayCount { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
    public User? MarkedByUser { get; set; }
    public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
}
