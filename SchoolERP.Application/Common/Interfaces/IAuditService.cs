namespace SchoolERP.Application.Common.Interfaces;

public interface IAuditService
{
    Task WriteAsync(
        string module,
        string action,
        string entityName,
        string? entityId,
        string outcome,
        string? details,
        Guid? tenantId,
        Guid? userId,
        CancellationToken cancellationToken);
}
