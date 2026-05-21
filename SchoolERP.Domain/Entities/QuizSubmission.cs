using SchoolERP.Domain.Common;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Domain.Entities;

public sealed class QuizSubmission : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid QuizId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public QuizSubmissionStatus Status { get; set; }
    public string AnswersJson { get; set; } = "[]";
    public decimal AutoEvaluatedMarks { get; set; }
    public decimal? ManualAdjustmentMarks { get; set; }

    public School? School { get; set; }
    public Quiz? Quiz { get; set; }
    public Student? Student { get; set; }
    public QuizResult? Result { get; set; }
}
