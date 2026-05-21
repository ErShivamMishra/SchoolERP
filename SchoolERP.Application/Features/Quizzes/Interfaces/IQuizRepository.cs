using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Quizzes.Interfaces;

public interface IQuizRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Subject?> GetSubjectByIdAsync(Guid subjectId, CancellationToken cancellationToken);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken cancellationToken);
    Task<Section?> GetSectionByIdAsync(Guid sectionId, CancellationToken cancellationToken);
    Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken);
    Task<Quiz?> GetQuizByIdAsync(Guid quizId, CancellationToken cancellationToken);
    Task<QuizSubmission?> GetSubmissionAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken);
    Task<QuizResult?> GetResultAsync(Guid quizId, Guid studentId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Quiz> Items, int TotalCount)> GetQuizPageAsync(Guid schoolId, Guid? classId, Guid? sectionId, Guid? subjectId, bool? isPublished, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<QuizResult>> GetLeaderboardAsync(Guid schoolId, Guid quizId, CancellationToken cancellationToken);
    Task<(int TotalQuizzes, int PublishedQuizzes, int TotalSubmissions, decimal AverageScore, int ActiveStudents)> GetAnalyticsAsync(Guid schoolId, CancellationToken cancellationToken);
    Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken);
    Task AddSubmissionAsync(QuizSubmission submission, CancellationToken cancellationToken);
    Task AddOrUpdateResultAsync(QuizResult result, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
