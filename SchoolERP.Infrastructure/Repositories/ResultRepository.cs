using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Results.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class ResultRepository(SchoolErpDbContext dbContext) : IResultRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken)
        => dbContext.Classes.FirstOrDefaultAsync(x => x.Id == classId, cancellationToken);

    public Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken)
        => dbContext.Sections.FirstOrDefaultAsync(x => x.Id == sectionId, cancellationToken);

    public Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid academicSessionId, CancellationToken cancellationToken)
        => dbContext.AcademicSessions.FirstOrDefaultAsync(x => x.Id == academicSessionId, cancellationToken);

    public async Task<IReadOnlyCollection<Subject>> GetSubjectsByIdsAsync(Guid schoolId, IReadOnlyCollection<Guid> subjectIds, CancellationToken cancellationToken)
        => await dbContext.Subjects.Where(x => x.SchoolId == schoolId && subjectIds.Contains(x.Id)).ToListAsync(cancellationToken);

    public Task<Exam?> GetExamByIdAsync(Guid examId, CancellationToken cancellationToken)
        => dbContext.Exams
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.AcademicSession)
            .Include(x => x.Subjects).ThenInclude(x => x.Subject)
            .Include(x => x.Results).ThenInclude(x => x.Student)
            .FirstOrDefaultAsync(x => x.Id == examId, cancellationToken);

    public async Task<(IReadOnlyCollection<Exam> Items, int TotalCount)> GetExamPageAsync(Guid schoolId, Guid? classId, Guid? sectionId, Guid? academicSessionId, bool? isPublished, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Exams
            .Include(x => x.Class)
            .Include(x => x.Section)
            .Include(x => x.AcademicSession)
            .Include(x => x.Subjects).ThenInclude(x => x.Subject)
            .Where(x => x.SchoolId == schoolId);

        if (classId.HasValue) query = query.Where(x => x.ClassId == classId.Value);
        if (sectionId.HasValue) query = query.Where(x => x.SectionId == sectionId.Value);
        if (academicSessionId.HasValue) query = query.Where(x => x.AcademicSessionId == academicSessionId.Value);
        if (isPublished.HasValue) query = query.Where(x => x.IsPublished == isPublished.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.StartDate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<IReadOnlyCollection<Student>> GetStudentsByIdsAsync(Guid schoolId, Guid classId, Guid? sectionId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken)
    {
        var query = dbContext.Students.Where(x => x.SchoolId == schoolId && x.ClassId == classId && studentIds.Contains(x.Id) && x.IsActive);
        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<ExamResult?> GetExamResultAsync(Guid examSubjectId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.ExamResults
            .Include(x => x.ExamSubject!).ThenInclude(x => x.Subject)
            .Include(x => x.Student)
            .FirstOrDefaultAsync(x => x.ExamSubjectId == examSubjectId && x.StudentId == studentId, cancellationToken);

    public async Task<IReadOnlyCollection<ExamResult>> GetExamResultsAsync(Guid examId, CancellationToken cancellationToken)
        => await dbContext.ExamResults
            .Include(x => x.ExamSubject!).ThenInclude(x => x.Subject)
            .Include(x => x.Student)
            .Where(x => x.ExamId == examId)
            .OrderBy(x => x.Student != null ? x.Student.RollNumber : string.Empty)
            .ToListAsync(cancellationToken);

    public async Task<(int TotalExams, int PublishedExams, int TotalResults, decimal AveragePercentage, int PassedResults)> GetAnalyticsAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var totalExams = await dbContext.Exams.CountAsync(x => x.SchoolId == schoolId, cancellationToken);
        var publishedExams = await dbContext.Exams.CountAsync(x => x.SchoolId == schoolId && x.IsPublished, cancellationToken);
        var totalResults = await dbContext.ExamResults.CountAsync(x => x.SchoolId == schoolId, cancellationToken);
        var averagePercentage = await dbContext.ExamResults
            .Where(x => x.SchoolId == schoolId)
            .Join(dbContext.ExamSubjects, result => result.ExamSubjectId, subject => subject.Id, (result, subject) => new { result.ObtainedMarks, subject.MaxMarks })
            .Select(x => x.MaxMarks == 0 ? 0 : (x.ObtainedMarks / x.MaxMarks) * 100m)
            .DefaultIfEmpty(0)
            .AverageAsync(cancellationToken);
        var passed = await dbContext.ExamResults
            .Where(x => x.SchoolId == schoolId)
            .Join(dbContext.ExamSubjects, result => result.ExamSubjectId, subject => subject.Id, (result, subject) => new { result.ObtainedMarks, subject.PassingMarks })
            .CountAsync(x => x.ObtainedMarks >= x.PassingMarks, cancellationToken);
        return (totalExams, publishedExams, totalResults, decimal.Round(averagePercentage, 2), passed);
    }

    public Task AddExamAsync(Exam exam, CancellationToken cancellationToken)
        => dbContext.Exams.AddAsync(exam, cancellationToken).AsTask();

    public Task AddOrUpdateExamResultAsync(ExamResult result, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(result).State == EntityState.Detached)
        {
            dbContext.ExamResults.Update(result);
        }

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
