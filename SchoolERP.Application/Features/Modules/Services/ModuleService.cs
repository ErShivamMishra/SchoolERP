using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Modules.Interfaces;
using SchoolERP.Application.Features.Modules.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Modules.Services;

public sealed class ModuleService(
    IModuleRepository moduleRepository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateModuleRequestDto> createValidator,
    IValidator<UpdateModuleRequestDto> updateValidator,
    IValidator<SetModuleActivationRequestDto> activationValidator) : IModuleService
{
    public async Task<ModuleDto> CreateAsync(CreateModuleRequestDto request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await moduleRepository.ExistsByCodeAsync(normalizedCode, null, cancellationToken))
        {
            throw new ConflictException("A module with this code already exists.", "module_code_exists");
        }

        var module = new Module
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Description = request.Description?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedBy = currentUserContext.UserId?.ToString()
        };

        await moduleRepository.AddAsync(module, cancellationToken);
        await moduleRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("ModuleManagement", "ModuleCreated", nameof(Module), module.Id.ToString(), "Success", $"Module {module.Code} created.", null, currentUserContext.UserId, cancellationToken);
        return Map(module);
    }

    public async Task<ModuleDto> UpdateAsync(Guid moduleId, UpdateModuleRequestDto request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var module = await moduleRepository.GetByIdAsync(moduleId, cancellationToken)
            ?? throw new NotFoundException("Module not found.", "module_not_found");

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await moduleRepository.ExistsByCodeAsync(normalizedCode, moduleId, cancellationToken))
        {
            throw new ConflictException("A module with this code already exists.", "module_code_exists");
        }

        module.Name = request.Name.Trim();
        module.Code = request.Code.Trim();
        module.Description = request.Description?.Trim();
        module.DisplayOrder = request.DisplayOrder;
        module.ModifiedAtUtc = DateTime.UtcNow;
        module.ModifiedBy = currentUserContext.UserId?.ToString();

        await moduleRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("ModuleManagement", "ModuleUpdated", nameof(Module), module.Id.ToString(), "Success", $"Module {module.Code} updated.", null, currentUserContext.UserId, cancellationToken);
        return Map(module);
    }

    public async Task<ModuleDto> SetActivationAsync(Guid moduleId, SetModuleActivationRequestDto request, CancellationToken cancellationToken)
    {
        await activationValidator.ValidateAndThrowAsync(request, cancellationToken);
        EnsureSuperAdmin();

        var module = await moduleRepository.GetByIdAsync(moduleId, cancellationToken)
            ?? throw new NotFoundException("Module not found.", "module_not_found");

        module.IsActive = request.IsActive;
        module.ModifiedAtUtc = DateTime.UtcNow;
        module.ModifiedBy = currentUserContext.UserId?.ToString();

        await moduleRepository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("ModuleManagement", request.IsActive ? "ModuleActivated" : "ModuleDeactivated", nameof(Module), module.Id.ToString(), "Success", $"Module active state set to {request.IsActive}.", null, currentUserContext.UserId, cancellationToken);
        return Map(module);
    }

    public async Task<IReadOnlyCollection<ModuleDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var modules = await moduleRepository.GetAllAsync(cancellationToken);
        return modules.Select(Map).ToArray();
    }

    private void EnsureSuperAdmin()
    {
        if (!currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            throw new ForbiddenException("Only SuperAdmin can manage modules.", "superadmin_required");
        }
    }

    private static ModuleDto Map(Module module)
    {
        return new ModuleDto
        {
            Id = module.Id,
            Name = module.Name,
            Code = module.Code,
            Description = module.Description,
            IsActive = module.IsActive,
            DisplayOrder = module.DisplayOrder
        };
    }
}
