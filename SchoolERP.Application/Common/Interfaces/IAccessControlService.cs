namespace SchoolERP.Application.Common.Interfaces;

public interface IAccessControlService
{
    Task EnsureModuleAccessAsync(string moduleCode, string permissionCode, CancellationToken cancellationToken);
}
