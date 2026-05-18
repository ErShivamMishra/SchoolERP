using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class SchoolRepository(SchoolErpDbContext dbContext) : ISchoolRepository
{
    public async Task<bool> ExistsByCodeAsync(string normalizedCode, CancellationToken cancellationToken)
    {
        return await dbContext.Schools.AnyAsync(x => x.Code.ToUpper() == normalizedCode, cancellationToken);
    }

    public async Task<School?> GetByIdAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        return await dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<School>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Schools
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(School school, CancellationToken cancellationToken)
    {
        return dbContext.Schools.AddAsync(school, cancellationToken).AsTask();
    }

    public async Task<Role?> GetRoleByCodeAsync(Guid? schoolId, string roleCode, CancellationToken cancellationToken)
    {
        return await dbContext.Roles.FirstOrDefaultAsync(x => x.TenantId == schoolId && x.Code == roleCode, cancellationToken);
    }

    public Task AddRoleAsync(Role role, CancellationToken cancellationToken)
    {
        return dbContext.Roles.AddAsync(role, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<Module>> GetModulesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Modules.Where(x => x.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Permission>> GetPermissionsByModuleIdsAsync(IReadOnlyCollection<Guid> moduleIds, CancellationToken cancellationToken)
    {
        return await dbContext.Permissions.Where(x => moduleIds.Contains(x.ModuleId)).ToListAsync(cancellationToken);
    }

    public Task AddRolePermissionsAsync(IEnumerable<RolePermission> rolePermissions, CancellationToken cancellationToken)
    {
        return dbContext.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
    }

    public Task AddUserAsync(User user, CancellationToken cancellationToken)
    {
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public async Task<SubscriptionPlan?> GetPlanByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Code == code && x.IsActive, cancellationToken);
    }

    public Task AddSubscriptionAsync(SchoolSubscription subscription, CancellationToken cancellationToken)
    {
        return dbContext.SchoolSubscriptions.AddAsync(subscription, cancellationToken).AsTask();
    }

    public async Task<SchoolSubscription?> GetLatestSubscriptionAsync(Guid schoolId, CancellationToken cancellationToken)
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
