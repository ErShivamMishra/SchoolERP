using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Schools.Interfaces;
using SchoolERP.Application.Features.Schools.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Schools.Services;

public sealed class SchoolService(
    ISchoolRepository schoolRepository,
    IPasswordHasher passwordHasher,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateSchoolRequestDto> createValidator,
    IValidator<UpdateSchoolRequestDto> updateValidator,
    IValidator<SetSchoolActivationRequestDto> activationValidator,
    IValidator<ExtendSchoolSubscriptionRequestDto> extendValidator) : ISchoolService
{
    public async Task<CreateSchoolResultDto> CreateAsync(CreateSchoolRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await schoolRepository.ExistsByCodeAsync(normalizedCode, cancellationToken))
        {
            throw new ConflictException("A school with this code already exists.", "school_code_exists");
        }

        var school = new School
        {
            Name = request.Name.Trim(),
            DisplayName = request.Name.Trim(),
            Code = request.Code.Trim(),
            Address = request.Address.Trim(),
            ContactEmail = request.ContactEmail.Trim(),
            ContactPhone = request.ContactPhone.Trim(),
            SubscriptionStartDateUtc = request.SubscriptionStartDate.ToUniversalTime(),
            SubscriptionEndDateUtc = request.SubscriptionEndDate.ToUniversalTime(),
            MaxStaffLimit = request.MaxStaffLimit,
            IsActive = true,
            Status = SchoolStatus.Active
        };

        await schoolRepository.AddAsync(school, cancellationToken);

        var schoolAdminRole = await EnsureTenantRoleAsync(school.Id, RoleNames.SchoolAdmin, cancellationToken);
        await EnsureTenantRoleAsync(school.Id, RoleNames.Staff, cancellationToken);
        await EnsureDefaultRolePermissionsAsync(schoolAdminRole.Id, cancellationToken);

        var temporaryPassword = GenerateTemporaryPassword();
        var schoolAdminEmail = $"admin@{request.Code.Trim().ToLowerInvariant()}.schoolerp.local";

        await schoolRepository.AddUserAsync(new User
        {
            TenantId = school.Id,
            FullName = $"{school.Name} Admin",
            Email = schoolAdminEmail,
            NormalizedEmail = schoolAdminEmail.ToUpperInvariant(),
            PhoneNumber = school.ContactPhone,
            PasswordHash = passwordHasher.HashPassword(temporaryPassword),
            RoleId = schoolAdminRole.Id,
            IsActive = true,
            IsPlatformUser = false,
            RequiresPasswordReset = true
        }, cancellationToken);

        var defaultPlan = await schoolRepository.GetPlanByCodeAsync("BASIC", cancellationToken)
            ?? throw new NotFoundException("Default subscription plan was not found.", "default_plan_missing");

        await schoolRepository.AddSubscriptionAsync(new SchoolSubscription
        {
            TenantId = school.Id,
            SubscriptionPlanId = defaultPlan.Id,
            Status = SubscriptionStatus.Active,
            StartDateUtc = school.SubscriptionStartDateUtc,
            EndDateUtc = school.SubscriptionEndDateUtc
        }, cancellationToken);

        await schoolRepository.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync("SchoolManagement", "SchoolCreated", nameof(School), school.Id.ToString(), "Success", $"School created with default SchoolAdmin {schoolAdminEmail}.", null, currentUserContext.UserId, cancellationToken);

        return new CreateSchoolResultDto
        {
            School = MapSchool(school),
            SchoolAdminEmail = schoolAdminEmail,
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<SchoolDto> UpdateAsync(Guid schoolId, UpdateSchoolRequestDto request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var school = await GetSchoolOrThrowAsync(schoolId, cancellationToken);
        school.Name = request.Name.Trim();
        school.DisplayName = request.Name.Trim();
        school.Address = request.Address.Trim();
        school.ContactEmail = request.ContactEmail.Trim();
        school.ContactPhone = request.ContactPhone.Trim();
        school.MaxStaffLimit = request.MaxStaffLimit;
        school.ModifiedAtUtc = DateTime.UtcNow;
        school.ModifiedBy = currentUserContext.UserId?.ToString();

        await schoolRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("SchoolManagement", "SchoolUpdated", nameof(School), school.Id.ToString(), "Success", "School details updated.", null, currentUserContext.UserId, cancellationToken);
        return MapSchool(school);
    }

    public async Task<SchoolDto> GetByIdAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        return MapSchool(await GetSchoolOrThrowAsync(schoolId, cancellationToken));
    }

    public async Task<IReadOnlyCollection<SchoolDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        EnsureSuperAdmin();
        var schools = await schoolRepository.GetAllAsync(cancellationToken);
        return schools.Select(MapSchool).ToArray();
    }

    public async Task<SchoolDto> SetActivationAsync(Guid schoolId, SetSchoolActivationRequestDto request, CancellationToken cancellationToken)
    {
        await activationValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var school = await GetSchoolOrThrowAsync(schoolId, cancellationToken);
        school.IsActive = request.IsActive;
        school.Status = request.IsActive ? SchoolStatus.Active : SchoolStatus.Suspended;
        school.ModifiedAtUtc = DateTime.UtcNow;
        school.ModifiedBy = currentUserContext.UserId?.ToString();

        await schoolRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("SchoolManagement", request.IsActive ? "SchoolActivated" : "SchoolDeactivated", nameof(School), school.Id.ToString(), "Success", $"School active state set to {request.IsActive}.", null, currentUserContext.UserId, cancellationToken);
        return MapSchool(school);
    }

    public async Task<SchoolDto> ExtendSubscriptionAsync(Guid schoolId, ExtendSchoolSubscriptionRequestDto request, CancellationToken cancellationToken)
    {
        await extendValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var school = await GetSchoolOrThrowAsync(schoolId, cancellationToken);
        var newEndDateUtc = request.NewSubscriptionEndDate.ToUniversalTime();
        if (newEndDateUtc <= school.SubscriptionEndDateUtc)
        {
            throw new BadRequestException("New subscription end date must be later than the current end date.", "invalid_subscription_extension");
        }

        school.SubscriptionEndDateUtc = newEndDateUtc;
        school.ModifiedAtUtc = DateTime.UtcNow;
        school.ModifiedBy = currentUserContext.UserId?.ToString();

        var latestSubscription = await schoolRepository.GetLatestSubscriptionAsync(schoolId, cancellationToken);
        if (latestSubscription is not null)
        {
            latestSubscription.EndDateUtc = newEndDateUtc;
            latestSubscription.Status = SubscriptionStatus.Active;
            latestSubscription.ModifiedAtUtc = DateTime.UtcNow;
            latestSubscription.ModifiedBy = currentUserContext.UserId?.ToString();
        }

        await schoolRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("SchoolManagement", "SubscriptionExtended", nameof(School), school.Id.ToString(), "Success", $"Subscription extended to {newEndDateUtc:O}.", null, currentUserContext.UserId, cancellationToken);
        return MapSchool(school);
    }

    private async Task<School> GetSchoolOrThrowAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        return await schoolRepository.GetByIdAsync(schoolId, cancellationToken)
            ?? throw new NotFoundException("School not found.", "school_not_found");
    }

    private async Task<Role> EnsureTenantRoleAsync(Guid schoolId, string roleCode, CancellationToken cancellationToken)
    {
        var existingRole = await schoolRepository.GetRoleByCodeAsync(schoolId, roleCode, cancellationToken);
        if (existingRole is not null)
        {
            return existingRole;
        }

        var role = new Role
        {
            TenantId = schoolId,
            Name = roleCode,
            Code = roleCode,
            IsSystemRole = true
        };

        await schoolRepository.AddRoleAsync(role, cancellationToken);
        return role;
    }

    private void EnsureSuperAdmin()
    {
        if (!currentUserContext.IsAuthenticated || !currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            throw new ForbiddenException("Only SuperAdmin can manage schools.", "superadmin_required");
        }
    }

    private static string GenerateTemporaryPassword()
    {
        return $"Temp@{Guid.NewGuid():N}"[..18];
    }

    private async Task EnsureDefaultRolePermissionsAsync(Guid schoolAdminRoleId, CancellationToken cancellationToken)
    {
        var modules = await schoolRepository.GetModulesAsync(cancellationToken);
        if (modules.Count == 0)
        {
            return;
        }

        var permissions = await schoolRepository.GetPermissionsByModuleIdsAsync(modules.Select(x => x.Id).ToArray(), cancellationToken);
        var rolePermissions = permissions.Select(permission => new RolePermission
        {
            RoleId = schoolAdminRoleId,
            PermissionId = permission.Id
        }).ToArray();

        if (rolePermissions.Length > 0)
        {
            await schoolRepository.AddRolePermissionsAsync(rolePermissions, cancellationToken);
        }
    }

    private static SchoolDto MapSchool(School school)
    {
        return new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            Code = school.Code,
            Address = school.Address,
            ContactEmail = school.ContactEmail,
            ContactPhone = school.ContactPhone,
            SubscriptionStartDate = school.SubscriptionStartDateUtc,
            SubscriptionEndDate = school.SubscriptionEndDateUtc,
            MaxStaffLimit = school.MaxStaffLimit,
            IsActive = school.IsActive,
            CreatedAt = school.CreatedAtUtc
        };
    }
}
