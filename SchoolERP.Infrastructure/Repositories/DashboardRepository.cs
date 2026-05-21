using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Dashboard.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class DashboardRepository(SchoolErpDbContext dbContext) : IDashboardRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public async Task<DashboardSnapshot> GetSnapshotAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var totalStudents = await dbContext.Students.CountAsync(x => !schoolId.HasValue || x.SchoolId == schoolId.Value, cancellationToken);
        var totalTeachers = await dbContext.Teachers.CountAsync(x => !schoolId.HasValue || x.SchoolId == schoolId.Value, cancellationToken);
        var activeSchools = schoolId.HasValue
            ? await dbContext.Schools.CountAsync(x => x.Id == schoolId.Value && x.IsActive, cancellationToken)
            : await dbContext.Schools.CountAsync(x => x.IsActive, cancellationToken);
        var attendanceSummaries = dbContext.AttendanceSummaries.Where(x => !schoolId.HasValue || x.SchoolId == schoolId.Value);
        var attendancePercentage = await attendanceSummaries.Select(x => (decimal?)x.AttendancePercentage).AverageAsync(cancellationToken) ?? 0;
        var pendingFees = await dbContext.Invoices.Where(x => !schoolId.HasValue || x.SchoolId == schoolId.Value).SumAsync(x => x.PendingAmount, cancellationToken);
        var monthlyRevenue = await dbContext.PaymentTransactions.Where(x => (!schoolId.HasValue || x.SchoolId == schoolId.Value) && x.PaymentDate.Year == now.Year && x.PaymentDate.Month == now.Month).SumAsync(x => x.Amount, cancellationToken);
        var totalSubmissions = await dbContext.QuizSubmissions.CountAsync(x => !schoolId.HasValue || x.SchoolId == schoolId.Value, cancellationToken);
        var totalPublishedQuizzes = await dbContext.Quizzes.CountAsync(x => (!schoolId.HasValue || x.SchoolId == schoolId.Value) && x.IsPublished, cancellationToken);
        var quizParticipation = totalPublishedQuizzes == 0 ? 0 : decimal.Round((totalSubmissions / (decimal)totalPublishedQuizzes), 2);
        var newAdmissions = await dbContext.Admissions.CountAsync(x => (!schoolId.HasValue || x.SchoolId == schoolId.Value) && x.CreatedAtUtc.Year == now.Year && x.CreatedAtUtc.Month == now.Month, cancellationToken);
        var subscriptionExpiryAlerts = schoolId.HasValue
            ? await dbContext.Schools.CountAsync(x => x.Id == schoolId.Value && x.SubscriptionEndDateUtc <= now.AddDays(30), cancellationToken)
            : await dbContext.Schools.CountAsync(x => x.SubscriptionEndDateUtc <= now.AddDays(30), cancellationToken);

        return new DashboardSnapshot
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            ActiveSchools = activeSchools,
            AttendancePercentage = decimal.Round(attendancePercentage, 2),
            PendingFees = pendingFees,
            MonthlyRevenue = monthlyRevenue,
            QuizParticipation = quizParticipation,
            NewAdmissions = newAdmissions,
            SubscriptionExpiryAlerts = subscriptionExpiryAlerts
        };
    }

    public Task<IReadOnlyCollection<DashboardMonthlyPoint>> GetAdmissionsTrendAsync(Guid? schoolId, CancellationToken cancellationToken)
        => GetMonthlyTrendAsync(
            dbContext.Admissions.Where(x => !schoolId.HasValue || x.SchoolId == schoolId.Value).GroupBy(x => new { x.CreatedAtUtc.Year, x.CreatedAtUtc.Month }).Select(x => new DashboardMonthlyPoint
            {
                Year = x.Key.Year,
                Month = x.Key.Month,
                Value = x.Count()
            }),
            cancellationToken);

    public Task<IReadOnlyCollection<DashboardMonthlyPoint>> GetRevenueTrendAsync(Guid? schoolId, CancellationToken cancellationToken)
        => GetMonthlyTrendAsync(
            dbContext.PaymentTransactions.Where(x => !schoolId.HasValue || x.SchoolId == schoolId.Value).GroupBy(x => new { x.PaymentDate.Year, x.PaymentDate.Month }).Select(x => new DashboardMonthlyPoint
            {
                Year = x.Key.Year,
                Month = x.Key.Month,
                Value = x.Sum(y => y.Amount)
            }),
            cancellationToken);

    public Task<IReadOnlyCollection<DashboardMonthlyPoint>> GetAttendanceTrendAsync(Guid? schoolId, CancellationToken cancellationToken)
        => GetMonthlyTrendAsync(
            dbContext.AttendanceSummaries.Where(x => !schoolId.HasValue || x.SchoolId == schoolId.Value).GroupBy(x => new { x.Year, x.Month }).Select(x => new DashboardMonthlyPoint
            {
                Year = x.Key.Year,
                Month = x.Key.Month,
                Value = x.Average(y => y.AttendancePercentage)
            }),
            cancellationToken);

    public async Task<(IReadOnlyCollection<AuditLog> Items, int TotalCount)> GetRecentActivityAsync(Guid? schoolId, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.AsQueryable();
        if (schoolId.HasValue)
        {
            query = query.Where(x => x.SchoolId == schoolId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Module.Contains(term) || x.Action.Contains(term) || x.EntityName.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    private static async Task<IReadOnlyCollection<DashboardMonthlyPoint>> GetMonthlyTrendAsync(IQueryable<DashboardMonthlyPoint> query, CancellationToken cancellationToken)
        => await query.OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync(cancellationToken);
}
