using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Modules.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class ModuleRepository(SchoolErpDbContext dbContext) : IModuleRepository
{
    public async Task<bool> ExistsByCodeAsync(string normalizedCode, Guid? excludeModuleId, CancellationToken cancellationToken)
    {
        return await dbContext.Modules.AnyAsync(x =>
            x.Code.ToUpper() == normalizedCode &&
            (!excludeModuleId.HasValue || x.Id != excludeModuleId.Value), cancellationToken);
    }

    public async Task<Module?> GetByIdAsync(Guid moduleId, CancellationToken cancellationToken)
    {
        return await dbContext.Modules.FirstOrDefaultAsync(x => x.Id == moduleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Module>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Modules
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Module module, CancellationToken cancellationToken)
    {
        return dbContext.Modules.AddAsync(module, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
