using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.AuditLogs.Models;

namespace SchoolERP.Application.Features.AuditLogs.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogListRequestDto request, CancellationToken cancellationToken);
}
