using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class ExamResult : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid ExamId { get; set; }
    public Guid ExamSubjectId { get; set; }
    public Guid StudentId { get; set; }
    public decimal ObtainedMarks { get; set; }
    public string? Grade { get; set; }
    public string? Remarks { get; set; }
    public bool IsPublished { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Exam? Exam { get; set; }
    public ExamSubject? ExamSubject { get; set; }
    public Student? Student { get; set; }
}
