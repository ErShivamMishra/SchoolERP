using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class QuizResult : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid QuizId { get; set; }
    public Guid StudentId { get; set; }
    public Guid QuizSubmissionId { get; set; }
    public decimal ObtainedMarks { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal Percentage { get; set; }
    public bool IsPassed { get; set; }
    public int Rank { get; set; }
    public string? EvaluatorRemarks { get; set; }

    public School? School { get; set; }
    public Quiz? Quiz { get; set; }
    public Student? Student { get; set; }
    public QuizSubmission? QuizSubmission { get; set; }
}
