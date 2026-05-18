using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Staff.Models;

namespace SchoolERP.Application.Features.Staff.Interfaces;

public interface IStaffService
{
    Task<StaffDto> CreateAsync(CreateStaffRequestDto request, CancellationToken cancellationToken);
    Task<StaffDto> UpdateAsync(Guid staffId, UpdateStaffRequestDto request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid staffId, CancellationToken cancellationToken);
    Task<StaffDto> SetActivationAsync(Guid staffId, SetStaffActivationRequestDto request, CancellationToken cancellationToken);
    Task<ResetStaffPasswordResultDto> ResetPasswordAsync(Guid staffId, CancellationToken cancellationToken);
    Task<StaffDto> GetByIdAsync(Guid staffId, CancellationToken cancellationToken);
    Task<PagedResult<StaffDto>> GetAllAsync(GetStaffListRequestDto request, CancellationToken cancellationToken);
}
