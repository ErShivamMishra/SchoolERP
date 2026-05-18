using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Staff.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class StaffRepository(SchoolErpDbContext dbContext) : IStaffRepository
{
    public async Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        return await dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);
    }

    public async Task<User?> GetStaffByIdAsync(Guid staffId, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == staffId, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<User> Items, int TotalCount)> GetStaffPageAsync(Guid schoolId, string? search, Guid? roleId, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .Include(x => x.Role)
            .Where(x => x.TenantId == schoolId && !x.IsPlatformUser);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(x =>
                x.FullName.Contains(normalizedSearch) ||
                x.Email.Contains(normalizedSearch) ||
                x.PhoneNumber.Contains(normalizedSearch));
        }

        if (roleId.HasValue)
        {
            query = query.Where(x => x.RoleId == roleId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> EmailExistsAsync(Guid schoolId, string normalizedEmail, Guid? excludeUserId, CancellationToken cancellationToken)
    {
        return await dbContext.Users.AnyAsync(x =>
            x.TenantId == schoolId &&
            x.NormalizedEmail == normalizedEmail &&
            (!excludeUserId.HasValue || x.Id != excludeUserId.Value), cancellationToken);
    }

    public async Task<int> GetActiveStaffCountAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        return await dbContext.Users.CountAsync(x => x.TenantId == schoolId && x.IsActive && !x.IsPlatformUser, cancellationToken);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
    }

    public async Task<Role?> GetRoleByCodeAsync(Guid? schoolId, string roleCode, CancellationToken cancellationToken)
    {
        return await dbContext.Roles.FirstOrDefaultAsync(x => x.TenantId == schoolId && x.Code == roleCode, cancellationToken);
    }

    public Task AddStaffAsync(User user, CancellationToken cancellationToken)
    {
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
