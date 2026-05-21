using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.AuditLogs.Interfaces;

public interface IAuditLogRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<AuditLog> Items, int TotalCount)> GetLogsPageAsync(Guid? schoolId, Guid? userId, string? module, string? action, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
