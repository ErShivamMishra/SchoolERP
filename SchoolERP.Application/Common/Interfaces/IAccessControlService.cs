namespace SchoolERP.Application.Common.Interfaces;

public interface IAccessControlService
{
    Task EnsureModuleAccessAsync(string moduleCode, string permissionAction, CancellationToken cancellationToken);
    Task<bool> HasModuleAccessAsync(Guid userId, Guid? schoolId, string roleName, string moduleCode, string permissionAction, CancellationToken cancellationToken);
    Task<bool> HasModuleEntitlementAsync(Guid schoolId, string moduleCode, CancellationToken cancellationToken);
}
