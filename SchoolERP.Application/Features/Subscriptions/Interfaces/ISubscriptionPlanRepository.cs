using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Subscriptions.Interfaces;

public interface ISubscriptionPlanRepository
{
    Task<bool> PlanCodeExistsAsync(string normalizedCode, Guid? excludePlanId, CancellationToken cancellationToken);
    Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken);
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Module>> GetModulesByIdsAsync(IReadOnlyCollection<Guid> moduleIds, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PlanModuleEntitlement>> GetPlanEntitlementsAsync(Guid planId, CancellationToken cancellationToken);
    Task AddPlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken);
    Task RemovePlanEntitlementsAsync(IEnumerable<PlanModuleEntitlement> entitlements, CancellationToken cancellationToken);
    Task AddPlanEntitlementsAsync(IEnumerable<PlanModuleEntitlement> entitlements, CancellationToken cancellationToken);
    Task AddSchoolSubscriptionAsync(SchoolSubscription subscription, CancellationToken cancellationToken);
    Task<SchoolSubscription?> GetLatestSchoolSubscriptionAsync(Guid schoolId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
