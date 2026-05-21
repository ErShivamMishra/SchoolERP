using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.AuditLogs.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class AuditLogRepository(SchoolErpDbContext dbContext) : IAuditLogRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);

    public async Task<(IReadOnlyCollection<AuditLog> Items, int TotalCount)> GetLogsPageAsync(Guid? schoolId, Guid? userId, string? module, string? action, string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.AuditLogs.AsQueryable();
        if (schoolId.HasValue)
        {
            query = query.Where(x => x.SchoolId == schoolId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(x => x.Module == module);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(x => x.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Module.Contains(term) || x.Action.Contains(term) || x.EntityName.Contains(term) || (x.Details != null && x.Details.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAtUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}
