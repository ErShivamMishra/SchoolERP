using SchoolERP.Application.Features.Modules.Models;

namespace SchoolERP.Application.Features.Modules.Interfaces;

public interface IModuleService
{
    Task<ModuleDto> CreateAsync(CreateModuleRequestDto request, CancellationToken cancellationToken);
    Task<ModuleDto> UpdateAsync(Guid moduleId, UpdateModuleRequestDto request, CancellationToken cancellationToken);
    Task<ModuleDto> SetActivationAsync(Guid moduleId, SetModuleActivationRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ModuleDto>> GetAllAsync(CancellationToken cancellationToken);
}
