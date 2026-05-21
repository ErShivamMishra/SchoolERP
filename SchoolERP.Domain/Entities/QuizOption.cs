using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class QuizOption : AuditableEntity
{
    public Guid QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }

    public QuizQuestion? Question { get; set; }
}
