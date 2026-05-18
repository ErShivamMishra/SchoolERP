using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Constants;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Services;

public sealed class AccessControlService(
    SchoolErpDbContext dbContext,
    ICurrentUserContext currentUserContext) : IAccessControlService
{
    public async Task EnsureModuleAccessAsync(string moduleCode, string permissionAction, CancellationToken cancellationToken)
    {
        if (!currentUserContext.IsAuthenticated || !currentUserContext.UserId.HasValue)
        {
            throw new UnauthorizedException("Authentication is required.", "authentication_required");
        }

        var hasAccess = await HasModuleAccessAsync(
            currentUserContext.UserId.Value,
            currentUserContext.SchoolId,
            currentUserContext.Role,
            moduleCode,
            permissionAction,
            cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenException("You do not have permission to access this module.", "module_access_denied");
        }
    }

    public async Task<bool> HasModuleAccessAsync(Guid userId, Guid? schoolId, string roleName, string moduleCode, string permissionAction, CancellationToken cancellationToken)
    {
        if (string.Equals(roleName, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!schoolId.HasValue)
        {
            return false;
        }

        if (!await HasModuleEntitlementAsync(schoolId.Value, moduleCode, cancellationToken))
        {
            return false;
        }

        var module = await dbContext.Modules.FirstOrDefaultAsync(x => x.Code == moduleCode && x.IsActive, cancellationToken);
        if (module is null)
        {
            return false;
        }

        var userPermission = await dbContext.UserPermissions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ModuleId == module.Id, cancellationToken);

        if (userPermission is not null)
        {
            return PermissionGranted(userPermission.CanView, userPermission.CanCreate, userPermission.CanEdit, userPermission.CanDelete, permissionAction);
        }

        var user = await dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user?.Role is null)
        {
            return false;
        }

        var grantedCodes = await dbContext.RolePermissions
            .Where(x => x.RoleId == user.RoleId)
            .Select(x => x.Permission!.Code)
            .ToListAsync(cancellationToken);

        return grantedCodes.Contains($"{moduleCode}.{permissionAction}");
    }

    public async Task<bool> HasModuleEntitlementAsync(Guid schoolId, string moduleCode, CancellationToken cancellationToken)
    {
        var schoolSubscription = await dbContext.SchoolSubscriptions
            .Include(x => x.SubscriptionPlan)
            .Where(x => x.TenantId == schoolId)
            .OrderByDescending(x => x.EndDateUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (schoolSubscription is null || schoolSubscription.EndDateUtc < DateTime.UtcNow || !schoolSubscription.SubscriptionPlan!.IsActive)
        {
            return false;
        }

        return await dbContext.PlanModuleEntitlements.AnyAsync(x =>
            x.SubscriptionPlanId == schoolSubscription.SubscriptionPlanId &&
            x.IsLicensed &&
            x.Module!.Code == moduleCode &&
            x.Module.IsActive, cancellationToken);
    }

    private static bool PermissionGranted(bool canView, bool canCreate, bool canEdit, bool canDelete, string permissionAction)
    {
        return permissionAction switch
        {
            PermissionActions.View => canView,
            PermissionActions.Create => canCreate,
            PermissionActions.Edit => canEdit,
            PermissionActions.Delete => canDelete,
            _ => false
        };
    }
}
