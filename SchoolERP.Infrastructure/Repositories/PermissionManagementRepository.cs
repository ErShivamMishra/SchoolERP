using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.AccessControl.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class PermissionManagementRepository(SchoolErpDbContext dbContext) : IPermissionManagementRepository
{
    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Module>> GetActiveModulesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Modules.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserPermission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.UserPermissions.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RolePermission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await dbContext.RolePermissions
            .Include(x => x.Permission)
            .ThenInclude(x => x!.Module)
            .Where(x => x.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public Task RemoveUserPermissionsAsync(IEnumerable<UserPermission> permissions, CancellationToken cancellationToken)
    {
        dbContext.UserPermissions.RemoveRange(permissions);
        return Task.CompletedTask;
    }

    public Task AddUserPermissionsAsync(IEnumerable<UserPermission> permissions, CancellationToken cancellationToken)
    {
        return dbContext.UserPermissions.AddRangeAsync(permissions, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
