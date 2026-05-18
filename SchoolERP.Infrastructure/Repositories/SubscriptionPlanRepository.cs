using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Subscriptions.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class SubscriptionPlanRepository(SchoolErpDbContext dbContext) : ISubscriptionPlanRepository
{
    public async Task<bool> PlanCodeExistsAsync(string normalizedCode, Guid? excludePlanId, CancellationToken cancellationToken)
    {
        return await dbContext.SubscriptionPlans.AnyAsync(x =>
            x.Code.ToUpper() == normalizedCode &&
            (!excludePlanId.HasValue || x.Id != excludePlanId.Value), cancellationToken);
    }

    public async Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken)
    {
        return await dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == planId, cancellationToken);
    }

    public async Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        return await dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Module>> GetModulesByIdsAsync(IReadOnlyCollection<Guid> moduleIds, CancellationToken cancellationToken)
    {
        return await dbContext.Modules.Where(x => moduleIds.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PlanModuleEntitlement>> GetPlanEntitlementsAsync(Guid planId, CancellationToken cancellationToken)
    {
        return await dbContext.PlanModuleEntitlements.Where(x => x.SubscriptionPlanId == planId).ToListAsync(cancellationToken);
    }

    public Task AddPlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken)
    {
        return dbContext.SubscriptionPlans.AddAsync(plan, cancellationToken).AsTask();
    }

    public Task RemovePlanEntitlementsAsync(IEnumerable<PlanModuleEntitlement> entitlements, CancellationToken cancellationToken)
    {
        dbContext.PlanModuleEntitlements.RemoveRange(entitlements);
        return Task.CompletedTask;
    }

    public Task AddPlanEntitlementsAsync(IEnumerable<PlanModuleEntitlement> entitlements, CancellationToken cancellationToken)
    {
        return dbContext.PlanModuleEntitlements.AddRangeAsync(entitlements, cancellationToken);
    }

    public Task AddSchoolSubscriptionAsync(SchoolSubscription subscription, CancellationToken cancellationToken)
    {
        return dbContext.SchoolSubscriptions.AddAsync(subscription, cancellationToken).AsTask();
    }

    public async Task<SchoolSubscription?> GetLatestSchoolSubscriptionAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        return await dbContext.SchoolSubscriptions
            .Where(x => x.TenantId == schoolId)
            .OrderByDescending(x => x.EndDateUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
