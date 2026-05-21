using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Services;

public sealed class AuditService(
    SchoolErpDbContext dbContext) : IAuditService
{
    public Task WriteAsync(
        string module,
        string action,
        string entityName,
        string? entityId,
        string outcome,
        string? details,
        Guid? tenantId,
        Guid? userId,
        CancellationToken cancellationToken)
        => WriteAsync(module, action, entityName, entityId, outcome, details, tenantId, userId, null, null, null, null, cancellationToken);

    public async Task WriteAsync(
        string module,
        string action,
        string entityName,
        string? entityId,
        string outcome,
        string? details,
        Guid? tenantId,
        Guid? userId,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        await dbContext.AuditLogs.AddAsync(new AuditLog
        {
            SchoolId = tenantId,
            UserId = userId,
            Module = module,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Outcome = outcome,
            Details = details,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
