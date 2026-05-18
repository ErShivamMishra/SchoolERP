using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Staff.Interfaces;
using SchoolERP.Application.Features.Staff.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Staff.Services;

public sealed class StaffService(
    IStaffRepository staffRepository,
    IPasswordHasher passwordHasher,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateStaffRequestDto> createValidator,
    IValidator<UpdateStaffRequestDto> updateValidator,
    IValidator<SetStaffActivationRequestDto> activationValidator,
    IValidator<GetStaffListRequestDto> listValidator) : IStaffService
{
    public async Task<StaffDto> CreateAsync(CreateStaffRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var schoolId = await ResolveSchoolIdForCreateAsync(request.SchoolId, cancellationToken);
        var school = await staffRepository.GetSchoolByIdAsync(schoolId, cancellationToken)
            ?? throw new NotFoundException("School not found.", "school_not_found");

        if (await staffRepository.GetActiveStaffCountAsync(schoolId, cancellationToken) >= school.MaxStaffLimit)
        {
            throw new BadRequestException("Maximum staff limit has been reached for this school.", "staff_limit_exceeded");
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        if (await staffRepository.EmailExistsAsync(schoolId, normalizedEmail, null, cancellationToken))
        {
            throw new ConflictException("A staff member with this email already exists in the school.", "staff_email_exists");
        }

        var role = await staffRepository.GetRoleByIdAsync(request.RoleId, cancellationToken)
            ?? throw new NotFoundException("Role not found.", "role_not_found");

        EnsureRoleAssignmentAllowed(role);

        var staff = new User
        {
            TenantId = schoolId,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PhoneNumber = request.PhoneNumber.Trim(),
            PasswordHash = passwordHasher.HashPassword(request.Password),
            RoleId = role.Id,
            IsActive = true,
            IsPlatformUser = false,
            RequiresPasswordReset = true,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await staffRepository.AddStaffAsync(staff, cancellationToken);
        await staffRepository.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(ModuleCodes.StaffManagement, "StaffCreated", nameof(User), staff.Id.ToString(), "Success", $"Staff user created with role {role.Name}.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapStaff(staff, role.Name);
    }

    public async Task<StaffDto> UpdateAsync(Guid staffId, UpdateStaffRequestDto request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var staff = await GetManagedStaffOrThrowAsync(staffId, cancellationToken);
        var role = await staffRepository.GetRoleByIdAsync(request.RoleId, cancellationToken)
            ?? throw new NotFoundException("Role not found.", "role_not_found");

        EnsureRoleAssignmentAllowed(role);

        var schoolId = staff.SchoolId ?? throw new ForbiddenException("School context is missing for this staff member.", "school_context_required");
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        if (await staffRepository.EmailExistsAsync(schoolId, normalizedEmail, staffId, cancellationToken))
        {
            throw new ConflictException("A staff member with this email already exists in the school.", "staff_email_exists");
        }

        staff.FullName = request.FullName.Trim();
        staff.Email = request.Email.Trim();
        staff.NormalizedEmail = normalizedEmail;
        staff.PhoneNumber = request.PhoneNumber.Trim();
        staff.RoleId = role.Id;
        staff.ModifiedAtUtc = DateTime.UtcNow;
        staff.ModifiedBy = currentUserContext.UserId?.ToString();

        await staffRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StaffManagement, "StaffUpdated", nameof(User), staff.Id.ToString(), "Success", "Staff details updated.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapStaff(staff, role.Name);
    }

    public async Task DeleteAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var staff = await GetManagedStaffOrThrowAsync(staffId, cancellationToken);
        if (staff.Role?.Code == RoleNames.SchoolAdmin && !currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            throw new ForbiddenException("SchoolAdmin cannot delete another SchoolAdmin.", "school_admin_delete_forbidden");
        }

        staff.IsDeleted = true;
        staff.IsActive = false;
        staff.ModifiedAtUtc = DateTime.UtcNow;
        staff.ModifiedBy = currentUserContext.UserId?.ToString();

        await staffRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StaffManagement, "StaffDeleted", nameof(User), staff.Id.ToString(), "Success", "Staff deleted via soft delete.", staff.SchoolId, currentUserContext.UserId, cancellationToken);
    }

    public async Task<StaffDto> SetActivationAsync(Guid staffId, SetStaffActivationRequestDto request, CancellationToken cancellationToken)
    {
        await activationValidator.ValidateAndThrowAsync(request, cancellationToken);

        var staff = await GetManagedStaffOrThrowAsync(staffId, cancellationToken);
        staff.IsActive = request.IsActive;
        staff.ModifiedAtUtc = DateTime.UtcNow;
        staff.ModifiedBy = currentUserContext.UserId?.ToString();

        await staffRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StaffManagement, request.IsActive ? "StaffActivated" : "StaffDeactivated", nameof(User), staff.Id.ToString(), "Success", $"Staff active state set to {request.IsActive}.", staff.SchoolId, currentUserContext.UserId, cancellationToken);
        return MapStaff(staff, staff.Role?.Name ?? string.Empty);
    }

    public async Task<ResetStaffPasswordResultDto> ResetPasswordAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var staff = await GetManagedStaffOrThrowAsync(staffId, cancellationToken);
        var temporaryPassword = $"Temp@{Guid.NewGuid():N}"[..18];

        staff.PasswordHash = passwordHasher.HashPassword(temporaryPassword);
        staff.RequiresPasswordReset = true;
        staff.FailedLoginAttempts = 0;
        staff.LockoutEndUtc = null;
        staff.ModifiedAtUtc = DateTime.UtcNow;
        staff.ModifiedBy = currentUserContext.UserId?.ToString();

        await staffRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.StaffManagement, "StaffPasswordReset", nameof(User), staff.Id.ToString(), "Success", "Staff password reset to a temporary password.", staff.SchoolId, currentUserContext.UserId, cancellationToken);

        return new ResetStaffPasswordResultDto
        {
            StaffId = staff.Id,
            TemporaryPassword = temporaryPassword
        };
    }

    public async Task<StaffDto> GetByIdAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var staff = await GetManagedStaffOrThrowAsync(staffId, cancellationToken);
        return MapStaff(staff, staff.Role?.Name ?? string.Empty);
    }

    public async Task<PagedResult<StaffDto>> GetAllAsync(GetStaffListRequestDto request, CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowAsync(request, cancellationToken);

        var schoolId = ResolveSchoolIdForRead(request.SchoolId);
        var (items, totalCount) = await staffRepository.GetStaffPageAsync(schoolId, request.Search?.Trim(), request.RoleId, request.IsActive, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<StaffDto>
        {
            Items = items.Select(x => MapStaff(x, x.Role?.Name ?? string.Empty)).ToArray(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private async Task<User> GetManagedStaffOrThrowAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetStaffByIdAsync(staffId, cancellationToken)
            ?? throw new NotFoundException("Staff member not found.", "staff_not_found");

        if (!staff.SchoolId.HasValue)
        {
            throw new ForbiddenException("Platform users cannot be managed through staff APIs.", "platform_user_forbidden");
        }

        EnsureSchoolAccess(staff.SchoolId.Value);
        return staff;
    }

    private async Task<Guid> ResolveSchoolIdForCreateAsync(Guid? requestedSchoolId, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            if (!requestedSchoolId.HasValue)
            {
                throw new BadRequestException("SchoolId is required for SuperAdmin staff creation.", "school_id_required");
            }

            return requestedSchoolId.Value;
        }

        if (!currentUserContext.Roles.Contains(RoleNames.SchoolAdmin) || !currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("Only SchoolAdmin or SuperAdmin can manage staff.", "staff_management_forbidden");
        }

        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("SchoolAdmin cannot manage staff for another school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private Guid ResolveSchoolIdForRead(Guid? requestedSchoolId)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            if (!requestedSchoolId.HasValue)
            {
                throw new BadRequestException("SchoolId is required for SuperAdmin staff queries.", "school_id_required");
            }

            return requestedSchoolId.Value;
        }

        if (!currentUserContext.Roles.Contains(RoleNames.SchoolAdmin) || !currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("Only SchoolAdmin or SuperAdmin can manage staff.", "staff_management_forbidden");
        }

        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("SchoolAdmin cannot view staff for another school.", "cross_tenant_access_forbidden");
        }

        return currentUserContext.SchoolId.Value;
    }

    private void EnsureSchoolAccess(Guid schoolId)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            return;
        }

        if (!currentUserContext.Roles.Contains(RoleNames.SchoolAdmin) || currentUserContext.SchoolId != schoolId)
        {
            throw new ForbiddenException("Staff access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private void EnsureRoleAssignmentAllowed(Role role)
    {
        if (role.Code == RoleNames.SuperAdmin && !currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            throw new ForbiddenException("SchoolAdmin cannot assign the SuperAdmin role.", "superadmin_role_forbidden");
        }

        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            if (!role.TenantId.HasValue || role.TenantId != currentUserContext.SchoolId)
            {
                throw new ForbiddenException("SchoolAdmin can assign roles only within the current school.", "cross_tenant_role_forbidden");
            }
        }
    }

    private static StaffDto MapStaff(User staff, string roleName)
    {
        return new StaffDto
        {
            Id = staff.Id,
            SchoolId = staff.SchoolId ?? Guid.Empty,
            FullName = staff.FullName,
            Email = staff.Email,
            PhoneNumber = staff.PhoneNumber,
            RoleId = staff.RoleId,
            RoleName = roleName,
            IsActive = staff.IsActive,
            IsFirstLogin = staff.IsFirstLogin,
            CreatedAt = staff.CreatedAtUtc
        };
    }
}
