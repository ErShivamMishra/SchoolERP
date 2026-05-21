using FluentValidation;
using SchoolERP.Application.Common.Models;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Quizzes.Models;

public sealed class CreateQuizOptionRequestDto
{
    public string OptionText { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
}

public sealed class CreateQuizQuestionRequestDto
{
    public string QuestionText { get; init; } = string.Empty;
    public QuizQuestionType QuestionType { get; init; } = QuizQuestionType.MultipleChoice;
    public decimal Marks { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyCollection<CreateQuizOptionRequestDto> Options { get; init; } = Array.Empty<CreateQuizOptionRequestDto>();
}

public sealed class CreateQuizRequestDto
{
    public Guid? SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid SubjectId { get; init; }
    public Guid ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public decimal TotalMarks { get; init; }
    public decimal PassingMarks { get; init; }
    public int DurationMinutes { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool RandomizeQuestions { get; init; }
    public IReadOnlyCollection<CreateQuizQuestionRequestDto> Questions { get; init; } = Array.Empty<CreateQuizQuestionRequestDto>();
}

public sealed class PublishQuizRequestDto
{
    public bool IsPublished { get; init; }
}

public sealed class QuizAnswerSubmissionDto
{
    public Guid QuestionId { get; init; }
    public Guid? SelectedOptionId { get; init; }
    public string? AnswerText { get; init; }
}

public sealed class SubmitQuizRequestDto
{
    public DateTime StartedAt { get; init; }
    public IReadOnlyCollection<QuizAnswerSubmissionDto> Answers { get; init; } = Array.Empty<QuizAnswerSubmissionDto>();
}

public sealed class ManualQuizEvaluationRequestDto
{
    public decimal ManualAdjustmentMarks { get; init; }
    public string? EvaluatorRemarks { get; init; }
}

public sealed class QuizDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid SubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public Guid ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public decimal TotalMarks { get; init; }
    public decimal PassingMarks { get; init; }
    public int DurationMinutes { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsPublished { get; init; }
    public bool RandomizeQuestions { get; init; }
    public IReadOnlyCollection<QuizQuestionDto> Questions { get; init; } = Array.Empty<QuizQuestionDto>();
}

public sealed class QuizQuestionDto
{
    public Guid Id { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public QuizQuestionType QuestionType { get; init; }
    public decimal Marks { get; init; }
    public int DisplayOrder { get; init; }
    public IReadOnlyCollection<QuizOptionDto> Options { get; init; } = Array.Empty<QuizOptionDto>();
}

public sealed class QuizOptionDto
{
    public Guid Id { get; init; }
    public string OptionText { get; init; } = string.Empty;
}

public sealed class QuizResultDto
{
    public Guid QuizId { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public decimal ObtainedMarks { get; init; }
    public decimal TotalMarks { get; init; }
    public decimal Percentage { get; init; }
    public bool IsPassed { get; init; }
    public int Rank { get; init; }
    public string? EvaluatorRemarks { get; init; }
}

public sealed class QuizLeaderboardDto
{
    public Guid QuizId { get; init; }
    public string QuizTitle { get; init; } = string.Empty;
    public IReadOnlyCollection<QuizResultDto> Rankings { get; init; } = Array.Empty<QuizResultDto>();
}

public sealed class QuizListRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public Guid? SubjectId { get; init; }
    public bool? IsPublished { get; init; }
}

public sealed class QuizAnalyticsDto
{
    public int TotalQuizzes { get; init; }
    public int PublishedQuizzes { get; init; }
    public int TotalSubmissions { get; init; }
    public decimal AverageScore { get; init; }
    public decimal ParticipationRate { get; init; }
}

public sealed class CreateQuizRequestDtoValidator : AbstractValidator<CreateQuizRequestDto>
{
    public CreateQuizRequestDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.TotalMarks).GreaterThan(0);
        RuleFor(x => x.PassingMarks).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 600);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate);
        RuleFor(x => x.Questions).NotEmpty();
        RuleForEach(x => x.Questions).ChildRules(question =>
        {
            question.RuleFor(x => x.QuestionText).NotEmpty().MaximumLength(2000);
            question.RuleFor(x => x.Marks).GreaterThan(0);
            question.RuleFor(x => x.Options).NotEmpty();
        });
    }
}

public sealed class PublishQuizRequestDtoValidator : AbstractValidator<PublishQuizRequestDto>
{
}

public sealed class SubmitQuizRequestDtoValidator : AbstractValidator<SubmitQuizRequestDto>
{
    public SubmitQuizRequestDtoValidator()
    {
        RuleFor(x => x.StartedAt).NotEmpty();
        RuleFor(x => x.Answers).NotEmpty();
    }
}

public sealed class ManualQuizEvaluationRequestDtoValidator : AbstractValidator<ManualQuizEvaluationRequestDto>
{
    public ManualQuizEvaluationRequestDtoValidator()
    {
        RuleFor(x => x.EvaluatorRemarks).MaximumLength(1000);
    }
}

public sealed class QuizListRequestDtoValidator : SearchablePagedRequestValidator<QuizListRequestDto>
{
}
