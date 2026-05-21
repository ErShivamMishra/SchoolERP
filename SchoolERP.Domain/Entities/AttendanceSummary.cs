using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class AttendanceSummary : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int HalfDays { get; set; }
    public decimal AttendancePercentage { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Student? Student { get; set; }
}
