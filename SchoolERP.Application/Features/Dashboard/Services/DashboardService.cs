using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Dashboard.Interfaces;
using SchoolERP.Application.Features.Dashboard.Models;
using SchoolERP.Domain.Constants;

namespace SchoolERP.Application.Features.Dashboard.Services;

public sealed class DashboardService(
    IDashboardRepository repository,
    ICurrentUserContext currentUserContext,
    IValidator<DashboardActivityRequestDto> activityValidator) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = await ResolveSchoolScopeAsync(schoolId, cancellationToken);
        var snapshot = await repository.GetSnapshotAsync(resolvedSchoolId, cancellationToken);
        return new DashboardSummaryDto
        {
            TotalStudents = snapshot.TotalStudents,
            TotalTeachers = snapshot.TotalTeachers,
            ActiveSchools = snapshot.ActiveSchools,
            AttendancePercentage = snapshot.AttendancePercentage,
            PendingFees = snapshot.PendingFees,
            MonthlyRevenue = snapshot.MonthlyRevenue,
            QuizParticipation = snapshot.QuizParticipation,
            NewAdmissions = snapshot.NewAdmissions,
            SubscriptionExpiryAlerts = snapshot.SubscriptionExpiryAlerts
        };
    }

    public async Task<DashboardAnalyticsDto> GetAnalyticsAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        var resolvedSchoolId = await ResolveSchoolScopeAsync(schoolId, cancellationToken);
        var admissions = await repository.GetAdmissionsTrendAsync(resolvedSchoolId, cancellationToken);
        var revenue = await repository.GetRevenueTrendAsync(resolvedSchoolId, cancellationToken);
        var attendance = await repository.GetAttendanceTrendAsync(resolvedSchoolId, cancellationToken);
        return new DashboardAnalyticsDto
        {
            AdmissionsTrend = admissions.Select(MapPoint).ToArray(),
            RevenueTrend = revenue.Select(MapPoint).ToArray(),
            AttendanceTrend = attendance.Select(MapPoint).ToArray()
        };
    }

    public async Task<PagedResult<RecentActivityDto>> GetRecentActivityAsync(DashboardActivityRequestDto request, CancellationToken cancellationToken)
    {
        await activityValidator.ValidateAndThrowAsync(request, cancellationToken);
        var resolvedSchoolId = await ResolveSchoolScopeAsync(request.SchoolId, cancellationToken);
        var (items, totalCount) = await repository.GetRecentActivityAsync(resolvedSchoolId, request.Search, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<RecentActivityDto>
        {
            Items = items.Select(x => new RecentActivityDto
            {
                Module = x.Module,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Outcome = x.Outcome,
                CreatedAt = x.CreatedAtUtc
            }).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private async Task<Guid?> ResolveSchoolScopeAsync(Guid? schoolId, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            if (schoolId.HasValue)
            {
                _ = await repository.GetSchoolByIdAsync(schoolId.Value, cancellationToken) ?? throw new NotFoundException("School not found.", "school_not_found");
            }

            return schoolId;
        }

        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }

        if (schoolId.HasValue && schoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Dashboard access is limited to the current school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private static DashboardTrendPointDto MapPoint(DashboardMonthlyPoint point) => new()
    {
        Label = $"{point.Year:D4}-{point.Month:D2}",
        Value = point.Value
    };
}
