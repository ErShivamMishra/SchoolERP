using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Quizzes.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class QuizRepository(SchoolErpDbContext dbContext) : IQuizRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<Subject?> GetSubjectByIdAsync(Guid subjectId, CancellationToken cancellationToken)
        => dbContext.Subjects.FirstOrDefaultAsync(x => x.Id == subjectId, cancellationToken);

    public Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken)
        => dbContext.Classes.FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);

    public Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken)
        => dbContext.Sections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

    public Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken)
        => dbContext.Students.FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);

    public Task<Quiz?> GetQuizByIdAsync(Guid quizId, CancellationToken cancellationToken)
        => dbContext.Quizzes
            .Include(x => x.Subject)
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.Questions).ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.Id == quizId, cancellationToken);

    public Task<QuizSubmission?> GetSubmissionAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.QuizSubmissions.FirstOrDefaultAsync(x => x.QuizId == quizId && x.StudentId == studentId, cancellationToken);

    public Task<QuizResult?> GetResultAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.QuizResults
            .Include(x => x.Student)
            .Include(x => x.QuizSubmission)
            .FirstOrDefaultAsync(x => x.QuizId == quizId && x.StudentId == studentId, cancellationToken);

    public async Task<(IReadOnlyCollection<Quiz> Items, int TotalCount)> GetQuizPageAsync(Guid schoolId, Guid? classId, Guid? sectionId, Guid? subjectId, bool? isPublished, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Quizzes
            .Include(x => x.Subject)
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.Questions).ThenInclude(x => x.Options)
            .Where(x => x.SchoolId == schoolId);

        if (classId.HasValue)
        {
            query = query.Where(x => x.ClassId == classId.Value);
        }

        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        if (subjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == subjectId.Value);
        }

        if (isPublished.HasValue)
        {
            query = query.Where(x => x.IsPublished == isPublished.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.Subject!.Name.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.StartDate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<IReadOnlyCollection<QuizResult>> GetLeaderboardAsync(Guid schoolId, Guid quizId, CancellationToken cancellationToken)
        => await dbContext.QuizResults
            .Include(x => x.Student)
            .Where(x => x.SchoolId == schoolId && x.QuizId == quizId)
            .OrderByDescending(x => x.ObtainedMarks)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<(int TotalQuizzes, int PublishedQuizzes, int TotalSubmissions, decimal AverageScore, int ActiveStudents)> GetAnalyticsAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var totalQuizzes = await dbContext.Quizzes.CountAsync(x => x.SchoolId == schoolId, cancellationToken);
        var publishedQuizzes = await dbContext.Quizzes.CountAsync(x => x.SchoolId == schoolId && x.IsPublished, cancellationToken);
        var totalSubmissions = await dbContext.QuizSubmissions.CountAsync(x => x.SchoolId == schoolId, cancellationToken);
        var averageScore = await dbContext.QuizResults.Where(x => x.SchoolId == schoolId).Select(x => (decimal?)x.Percentage).AverageAsync(cancellationToken) ?? 0;
        var activeStudents = await dbContext.Students.CountAsync(x => x.SchoolId == schoolId && x.IsActive, cancellationToken);
        return (totalQuizzes, publishedQuizzes, totalSubmissions, decimal.Round(averageScore, 2), activeStudents);
    }

    public Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken)
        => dbContext.Quizzes.AddAsync(quiz, cancellationToken).AsTask();

    public Task AddSubmissionAsync(QuizSubmission submission, CancellationToken cancellationToken)
        => dbContext.QuizSubmissions.AddAsync(submission, cancellationToken).AsTask();

    public Task AddOrUpdateResultAsync(QuizResult result, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(result).State == EntityState.Detached)
        {
            dbContext.QuizResults.Update(result);
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
