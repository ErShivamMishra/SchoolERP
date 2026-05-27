using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Results.Interfaces;

public interface IResultRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken);
    Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken);
    Task<AcademicSession?> GetAcademicSessionByIdAsync(Guid academicSessionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Subject>> GetSubjectsByIdsAsync(Guid schoolId, IReadOnlyCollection<Guid> subjectIds, CancellationToken cancellationToken);
    Task<Exam?> GetExamByIdAsync(Guid examId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Exam> Items, int TotalCount)> GetExamPageAsync(Guid schoolId, Guid? classId, Guid? sectionId, Guid? academicSessionId, bool? isPublished, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Student>> GetStudentsByIdsAsync(Guid schoolId, Guid classId, Guid? sectionId, IReadOnlyCollection<Guid> studentIds, CancellationToken cancellationToken);
    Task<ExamResult?> GetExamResultAsync(Guid examSubjectId, Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ExamResult>> GetExamResultsAsync(Guid examId, CancellationToken cancellationToken);
    Task<(int TotalExams, int PublishedExams, int TotalResults, decimal AveragePercentage, int PassedResults)> GetAnalyticsAsync(Guid schoolId, CancellationToken cancellationToken);
    Task AddExamAsync(Exam exam, CancellationToken cancellationToken);
    Task AddOrUpdateExamResultAsync(ExamResult result, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
