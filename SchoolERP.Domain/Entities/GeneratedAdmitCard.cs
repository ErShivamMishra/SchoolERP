using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class GeneratedAdmitCard : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid TemplateId { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string? RoomNumber { get; set; }
    public string? Instructions { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

    public School? School { get; set; }
    public AdmitCardTemplate? Template { get; set; }
    public Exam? Exam { get; set; }
    public Student? Student { get; set; }
}
