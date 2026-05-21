using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Quiz : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublished { get; set; }
    public bool RandomizeQuestions { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Subject? Subject { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public ICollection<QuizSubmission> Submissions { get; set; } = new List<QuizSubmission>();
    public ICollection<QuizResult> Results { get; set; } = new List<QuizResult>();
}
