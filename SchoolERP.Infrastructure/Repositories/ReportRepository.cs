using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Reports.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class ReportRepository(SchoolErpDbContext dbContext) : IReportRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public async Task<(IReadOnlyCollection<Student> Items, int TotalCount)> GetStudentsPageAsync(Guid schoolId, Guid? classId, Guid? sectionId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Students.Include(x => x.Class).Include(x => x.Section).Where(x => x.SchoolId == schoolId);
        if (classId.HasValue)
        {
            query = query.Where(x => x.ClassId == classId.Value);
        }

        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.RollNumber.Contains(term) || x.FirstName.Contains(term) || x.LastName.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.RollNumber).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(IReadOnlyCollection<AttendanceRecord> Items, int TotalCount)> GetAttendancePageAsync(Guid schoolId, DateTime? fromDate, DateTime? toDate, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.AttendanceRecords.Include(x => x.Student).Include(x => x.Class).Include(x => x.Section).Where(x => x.SchoolId == schoolId);
        if (fromDate.HasValue)
        {
            query = query.Where(x => x.AttendanceDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.AttendanceDate <= toDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Student!.FirstName.Contains(term) || x.Student.LastName.Contains(term) || x.Student.RollNumber.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.AttendanceDate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(IReadOnlyCollection<Invoice> Items, int TotalCount)> GetInvoicePageAsync(Guid schoolId, Guid? studentId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Invoices.Include(x => x.Student).Where(x => x.SchoolId == schoolId);
        if (studentId.HasValue)
        {
            query = query.Where(x => x.StudentId == studentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.InvoiceNumber.Contains(term) || x.Student!.FirstName.Contains(term) || x.Student.RollNumber.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.InvoiceDate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(IReadOnlyCollection<QuizResult> Items, int TotalCount)> GetQuizResultPageAsync(Guid schoolId, Guid quizId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.QuizResults.Include(x => x.Student).Where(x => x.SchoolId == schoolId && x.QuizId == quizId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Student!.FirstName.Contains(term) || x.Student.LastName.Contains(term) || x.Student.RollNumber.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Rank).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
