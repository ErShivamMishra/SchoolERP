using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.AccessControl.Models;

namespace SchoolERP.Application.Features.AccessControl.Interfaces;

public interface IPermissionManagementService
{
    Task<IReadOnlyCollection<ModulePermissionDto>> AssignUserPermissionsAsync(Guid userId, UpsertUserPermissionsRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ModulePermissionDto>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ModulePermissionDto>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken);
}
