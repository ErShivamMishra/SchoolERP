using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.AccessControl.Interfaces;
using SchoolERP.Application.Features.AccessControl.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.AccessControl.Services;

public sealed class PermissionManagementService(
    IPermissionManagementRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<UpsertUserPermissionsRequestDto> validator) : IPermissionManagementService
{
    public async Task<IReadOnlyCollection<ModulePermissionDto>> AssignUserPermissionsAsync(Guid userId, UpsertUserPermissionsRequestDto request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await repository.GetUserByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.", "user_not_found");

        EnsureManageableUser(user);

        var modules = await repository.GetActiveModulesAsync(cancellationToken);
        var modulesById = modules.ToDictionary(x => x.Id);

        foreach (var permission in request.Permissions)
        {
            if (!modulesById.ContainsKey(permission.ModuleId))
            {
                throw new NotFoundException("One or more modules were not found.", "module_not_found");
            }
        }

        var existing = await repository.GetUserPermissionsAsync(userId, cancellationToken);
        await repository.RemoveUserPermissionsAsync(existing, cancellationToken);

        var newPermissions = request.Permissions.Select(permission => new UserPermission
        {
            UserId = userId,
            ModuleId = permission.ModuleId,
            CanView = permission.CanView,
            CanCreate = permission.CanCreate,
            CanEdit = permission.CanEdit,
            CanDelete = permission.CanDelete,
            CreatedBy = currentUserContext.UserId?.ToString()
        }).ToArray();

        await repository.AddUserPermissionsAsync(newPermissions, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(ModuleCodes.StaffManagement, "UserPermissionsUpdated", nameof(UserPermission), userId.ToString(), "Success", "User permissions updated.", user.SchoolId, currentUserContext.UserId, cancellationToken);
        return newPermissions.Select(permission => MapPermission(permission, modulesById[permission.ModuleId])).ToArray();
    }

    public async Task<IReadOnlyCollection<ModulePermissionDto>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.", "user_not_found");

        EnsureManageableUser(user);

        var modules = await repository.GetActiveModulesAsync(cancellationToken);
        var modulesById = modules.ToDictionary(x => x.Id);
        var permissions = await repository.GetUserPermissionsAsync(userId, cancellationToken);
        return permissions
            .Where(x => modulesById.ContainsKey(x.ModuleId))
            .Select(x => MapPermission(x, modulesById[x.ModuleId]))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<ModulePermissionDto>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await repository.GetRoleByIdAsync(roleId, cancellationToken)
            ?? throw new NotFoundException("Role not found.", "role_not_found");

        EnsureRoleReadable(role);

        var rolePermissions = await repository.GetRolePermissionsAsync(roleId, cancellationToken);
        return rolePermissions
            .Where(x => x.Permission?.Module is not null)
            .GroupBy(x => x.Permission!.Module!)
            .Select(group => new ModulePermissionDto
            {
                ModuleId = group.Key.Id,
                ModuleName = group.Key.Name,
                ModuleCode = group.Key.Code,
                CanView = group.Any(x => x.Permission!.Code == $"{group.Key.Code}.{PermissionActions.View}"),
                CanCreate = group.Any(x => x.Permission!.Code == $"{group.Key.Code}.{PermissionActions.Create}"),
                CanEdit = group.Any(x => x.Permission!.Code == $"{group.Key.Code}.{PermissionActions.Edit}"),
                CanDelete = group.Any(x => x.Permission!.Code == $"{group.Key.Code}.{PermissionActions.Delete}")
            })
            .OrderBy(x => x.ModuleName)
            .ToArray();
    }

    private void EnsureManageableUser(User user)
    {
        if (user.IsPlatformUser && !currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            throw new ForbiddenException("Platform users cannot be managed by SchoolAdmin.", "platform_user_forbidden");
        }

        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            return;
        }

        if (!currentUserContext.Roles.Contains(RoleNames.SchoolAdmin) || currentUserContext.SchoolId != user.SchoolId)
        {
            throw new ForbiddenException("User permissions can be managed only within the current school.", "cross_tenant_access_forbidden");
        }
    }

    private void EnsureRoleReadable(Role role)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            return;
        }

        if (!currentUserContext.Roles.Contains(RoleNames.SchoolAdmin) || role.TenantId != currentUserContext.SchoolId)
        {
            throw new ForbiddenException("Role permissions can be viewed only within the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static ModulePermissionDto MapPermission(UserPermission permission, Module module)
    {
        return new ModulePermissionDto
        {
            ModuleId = module.Id,
            ModuleName = module.Name,
            ModuleCode = module.Code,
            CanView = permission.CanView,
            CanCreate = permission.CanCreate,
            CanEdit = permission.CanEdit,
            CanDelete = permission.CanDelete
        };
    }
}
