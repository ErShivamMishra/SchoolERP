using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class Exam : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid AcademicSessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublished { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
    public AcademicSession? AcademicSession { get; set; }
    public ICollection<ExamSubject> Subjects { get; set; } = new List<ExamSubject>();
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}
