using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Services;

public sealed class AuditService(SchoolErpDbContext dbContext) : IAuditService
{
    public async Task WriteAsync(string module, string action, string entityName, string? entityId, string outcome, string? details, Guid? tenantId, Guid? userId, CancellationToken cancellationToken)
    {
        await dbContext.AuditLogs.AddAsync(new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            Module = module,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Outcome = outcome,
            Details = details
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
