using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Subscriptions.Interfaces;
using SchoolERP.Application.Features.Subscriptions.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Subscriptions.Services;

public sealed class SubscriptionPlanService(
    ISubscriptionPlanRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateSubscriptionPlanRequestDto> createValidator,
    IValidator<AssignPlanModulesRequestDto> assignModulesValidator,
    IValidator<AssignSchoolPlanRequestDto> assignSchoolPlanValidator) : ISubscriptionPlanService
{
    public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await repository.PlanCodeExistsAsync(normalizedCode, null, cancellationToken))
        {
            throw new ConflictException("A subscription plan with this code already exists.", "plan_code_exists");
        }

        var plan = new SubscriptionPlan
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Price = request.Price,
            IsActive = true,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await repository.AddPlanAsync(plan, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync("SubscriptionPlanManagement", "PlanCreated", nameof(SubscriptionPlan), plan.Id.ToString(), "Success", $"Plan {plan.Code} created.", null, currentUserContext.UserId, cancellationToken);
        return MapPlan(plan);
    }

    public async Task<IReadOnlyCollection<PlanModuleDto>> AssignModulesAsync(Guid planId, AssignPlanModulesRequestDto request, CancellationToken cancellationToken)
    {
        await assignModulesValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var plan = await repository.GetPlanByIdAsync(planId, cancellationToken)
            ?? throw new NotFoundException("Subscription plan not found.", "plan_not_found");

        var moduleIds = request.Modules.Select(x => x.ModuleId).Distinct().ToArray();
        var modules = await repository.GetModulesByIdsAsync(moduleIds, cancellationToken);
        var modulesById = modules.ToDictionary(x => x.Id);

        if (modulesById.Count != moduleIds.Length)
        {
            throw new NotFoundException("One or more modules were not found.", "module_not_found");
        }

        var existing = await repository.GetPlanEntitlementsAsync(planId, cancellationToken);
        await repository.RemovePlanEntitlementsAsync(existing, cancellationToken);

        var entitlements = request.Modules.Select(module => new PlanModuleEntitlement
        {
            SubscriptionPlanId = planId,
            ModuleId = module.ModuleId,
            IsLicensed = module.IsLicensed,
            IsVisibleInMenu = module.IsVisibleInMenu
        }).ToArray();

        await repository.AddPlanEntitlementsAsync(entitlements, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync("SubscriptionPlanManagement", "PlanModulesAssigned", nameof(PlanModuleEntitlement), planId.ToString(), "Success", $"Updated {entitlements.Length} module entitlements for plan {plan.Code}.", null, currentUserContext.UserId, cancellationToken);

        return entitlements.Select(entitlement => new PlanModuleDto
        {
            ModuleId = entitlement.ModuleId,
            ModuleName = modulesById[entitlement.ModuleId].Name,
            ModuleCode = modulesById[entitlement.ModuleId].Code,
            IsLicensed = entitlement.IsLicensed,
            IsVisibleInMenu = entitlement.IsVisibleInMenu
        }).ToArray();
    }

    public async Task<SchoolSubscriptionDto> AssignPlanToSchoolAsync(Guid schoolId, AssignSchoolPlanRequestDto request, CancellationToken cancellationToken)
    {
        await assignSchoolPlanValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var school = await repository.GetSchoolByIdAsync(schoolId, cancellationToken)
            ?? throw new NotFoundException("School not found.", "school_not_found");

        var plan = await repository.GetPlanByIdAsync(request.SubscriptionPlanId, cancellationToken)
            ?? throw new NotFoundException("Subscription plan not found.", "plan_not_found");

        var subscription = new SchoolSubscription
        {
            TenantId = schoolId,
            SubscriptionPlanId = plan.Id,
            Status = SubscriptionStatus.Active,
            StartDateUtc = request.StartDate.ToUniversalTime(),
            EndDateUtc = request.EndDate.ToUniversalTime(),
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        school.SubscriptionStartDateUtc = subscription.StartDateUtc;
        school.SubscriptionEndDateUtc = subscription.EndDateUtc;
        school.ModifiedAtUtc = DateTime.UtcNow;
        school.ModifiedBy = currentUserContext.UserId?.ToString();

        var latest = await repository.GetLatestSchoolSubscriptionAsync(schoolId, cancellationToken);
        if (latest is not null)
        {
            latest.Status = latest.EndDateUtc < DateTime.UtcNow ? SubscriptionStatus.Expired : SubscriptionStatus.Suspended;
            latest.ModifiedAtUtc = DateTime.UtcNow;
            latest.ModifiedBy = currentUserContext.UserId?.ToString();
        }

        await repository.AddSchoolSubscriptionAsync(subscription, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync("SubscriptionPlanManagement", "SchoolPlanAssigned", nameof(SchoolSubscription), subscription.Id.ToString(), "Success", $"Assigned plan {plan.Code} to school {school.Code}.", schoolId, currentUserContext.UserId, cancellationToken);

        return new SchoolSubscriptionDto
        {
            SchoolId = schoolId,
            SubscriptionPlanId = plan.Id,
            SubscriptionPlanCode = plan.Code,
            StartDate = subscription.StartDateUtc,
            EndDate = subscription.EndDateUtc
        };
    }

    private void EnsureSuperAdmin()
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            throw new ForbiddenException("Only SuperAdmin can manage subscription plans.", "superadmin_required");
        }
    }

    private static SubscriptionPlanDto MapPlan(SubscriptionPlan plan)
    {
        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Code = plan.Code,
            Price = plan.Price,
            IsActive = plan.IsActive
        };
    }
}
