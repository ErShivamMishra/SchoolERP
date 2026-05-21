using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Reports.Interfaces;

public interface IReportRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Student> Items, int TotalCount)> GetStudentsPageAsync(Guid schoolId, Guid? classId, Guid? sectionId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<AttendanceRecord> Items, int TotalCount)> GetAttendancePageAsync(Guid schoolId, DateTime? fromDate, DateTime? toDate, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Invoice> Items, int TotalCount)> GetInvoicePageAsync(Guid schoolId, Guid? studentId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<QuizResult> Items, int TotalCount)> GetQuizResultPageAsync(Guid schoolId, Guid quizId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
