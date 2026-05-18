using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Schools.Interfaces;

public interface ISchoolRepository
{
    Task<bool> ExistsByCodeAsync(string normalizedCode, CancellationToken cancellationToken);
    Task<School?> GetByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<School>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(School school, CancellationToken cancellationToken);
    Task<Role?> GetRoleByCodeAsync(Guid? schoolId, string roleCode, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Module>> GetModulesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Permission>> GetPermissionsByModuleIdsAsync(IReadOnlyCollection<Guid> moduleIds, CancellationToken cancellationToken);
    Task AddRoleAsync(Role role, CancellationToken cancellationToken);
    Task AddRolePermissionsAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken);
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task<SubscriptionPlan?> GetPlanByCodeAsync(string code, CancellationToken cancellationToken);
    Task AddSubscriptionAsync(SchoolSubscription subscription, CancellationToken cancellationToken);
    Task<SchoolSubscription?> GetLatestSubscriptionAsync(Guid schoolId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
