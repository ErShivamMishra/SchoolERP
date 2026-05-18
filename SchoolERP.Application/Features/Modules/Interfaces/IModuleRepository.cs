using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Modules.Interfaces;

public interface IModuleRepository
{
    Task<bool> ExistsByCodeAsync(string normalizedCode, Guid? excludeModuleId, CancellationToken cancellationToken);
    Task<Module?> GetByIdAsync(Guid moduleId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Module>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Module module, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
