using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.AccessControl.Interfaces;

public interface IPermissionManagementRepository
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Module>> GetActiveModulesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserPermission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RolePermission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken);
    Task RemoveUserPermissionsAsync(IEnumerable<UserPermission> permissions, CancellationToken cancellationToken);
    Task AddUserPermissionsAsync(IEnumerable<UserPermission> permissions, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
