using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class ExamSubject : AuditableEntity
{
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public DateTime ExamDate { get; set; }

    public Exam? Exam { get; set; }
    public Subject? Subject { get; set; }
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}
