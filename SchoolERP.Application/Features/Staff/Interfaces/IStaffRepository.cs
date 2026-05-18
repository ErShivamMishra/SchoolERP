using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Staff.Interfaces;

public interface IStaffRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<User?> GetStaffByIdAsync(Guid staffId, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<User> Items, int TotalCount)> GetStaffPageAsync(Guid schoolId, string? search, Guid? roleId, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(Guid schoolId, string normalizedEmail, Guid? excludeUserId, CancellationToken cancellationToken);
    Task<int> GetActiveStaffCountAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken);
    Task<Role?> GetRoleByCodeAsync(Guid? schoolId, string roleCode, CancellationToken cancellationToken);
    Task AddStaffAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
