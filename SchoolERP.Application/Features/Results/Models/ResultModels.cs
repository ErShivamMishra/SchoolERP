using FluentValidation;
using SchoolERP.Application.Common.Models;

namespace SchoolERP.Application.Features.Results.Models;

public sealed class ExamSubjectRequestDto
{
    public Guid SubjectId { get; init; }
    public decimal MaxMarks { get; init; }
    public decimal PassingMarks { get; init; }
    public DateTime ExamDate { get; init; }
}

public sealed class CreateExamRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public Guid AcademicSessionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IReadOnlyCollection<ExamSubjectRequestDto> Subjects { get; init; } = Array.Empty<ExamSubjectRequestDto>();
}

public sealed class PublishExamRequestDto
{
    public bool IsPublished { get; init; }
}

public sealed class RecordExamResultEntryDto
{
    public Guid ExamSubjectId { get; init; }
    public Guid StudentId { get; init; }
    public decimal ObtainedMarks { get; init; }
    public string? Grade { get; init; }
    public string? Remarks { get; init; }
    public bool IsPublished { get; init; }
}

public sealed class RecordExamResultsRequestDto
{
    public IReadOnlyCollection<RecordExamResultEntryDto> Results { get; init; } = Array.Empty<RecordExamResultEntryDto>();
}

public sealed class ExamSubjectDto
{
    public Guid Id { get; init; }
    public Guid SubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public decimal MaxMarks { get; init; }
    public decimal PassingMarks { get; init; }
    public DateTime ExamDate { get; init; }
}

public sealed class ExamDto
{
    public Guid Id { get; init; }
    public Guid SchoolId { get; init; }
    public Guid ClassId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public Guid? SectionId { get; init; }
    public string? SectionName { get; init; }
    public Guid AcademicSessionId { get; init; }
    public string AcademicSessionName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsPublished { get; init; }
    public IReadOnlyCollection<ExamSubjectDto> Subjects { get; init; } = Array.Empty<ExamSubjectDto>();
}

public sealed class ExamResultDto
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public Guid ExamSubjectId { get; init; }
    public string SubjectName { get; init; } = string.Empty;
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public decimal ObtainedMarks { get; init; }
    public decimal MaxMarks { get; init; }
    public decimal PassingMarks { get; init; }
    public string? Grade { get; init; }
    public string? Remarks { get; init; }
    public bool IsPublished { get; init; }
}

public sealed class StudentExamReportDto
{
    public Guid ExamId { get; init; }
    public string ExamTitle { get; init; } = string.Empty;
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public decimal TotalObtainedMarks { get; init; }
    public decimal TotalMaxMarks { get; init; }
    public decimal Percentage { get; init; }
    public IReadOnlyCollection<ExamResultDto> SubjectResults { get; init; } = Array.Empty<ExamResultDto>();
}

public sealed class GetExamListRequestDto : SearchablePagedRequest
{
    public Guid? SchoolId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? SectionId { get; init; }
    public Guid? AcademicSessionId { get; init; }
    public bool? IsPublished { get; init; }
}

public sealed class ResultAnalyticsDto
{
    public int TotalExams { get; init; }
    public int PublishedExams { get; init; }
    public int TotalResults { get; init; }
    public decimal AveragePercentage { get; init; }
    public decimal PassRate { get; init; }
}

public sealed class CreateExamRequestDtoValidator : AbstractValidator<CreateExamRequestDto>
{
    public CreateExamRequestDtoValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.AcademicSessionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.Subjects).NotEmpty();
    }
}

public sealed class RecordExamResultsRequestDtoValidator : AbstractValidator<RecordExamResultsRequestDto>
{
    public RecordExamResultsRequestDtoValidator()
    {
        RuleFor(x => x.Results).NotEmpty();
        RuleForEach(x => x.Results).ChildRules(result =>
        {
            result.RuleFor(x => x.ExamSubjectId).NotEmpty();
            result.RuleFor(x => x.StudentId).NotEmpty();
            result.RuleFor(x => x.ObtainedMarks).GreaterThanOrEqualTo(0);
            result.RuleFor(x => x.Grade).MaximumLength(20);
            result.RuleFor(x => x.Remarks).MaximumLength(500);
        });
    }
}

public sealed class GetExamListRequestDtoValidator : SearchablePagedRequestValidator<GetExamListRequestDto>
{
}
