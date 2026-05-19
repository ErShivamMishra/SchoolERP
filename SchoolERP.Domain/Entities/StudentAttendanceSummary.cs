using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class StudentAttendanceSummary : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int LeaveCount { get; set; }
    public int HalfDayCount { get; set; }

    public School? School { get; set; }
    public Student? Student { get; set; }
}
