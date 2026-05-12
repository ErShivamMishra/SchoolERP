using SchoolERP.Application.Features.Schools.Models;

namespace SchoolERP.Application.Features.Schools.Interfaces;

public interface ISchoolService
{
    Task<CreateSchoolResultDto> CreateAsync(CreateSchoolRequestDto request, CancellationToken cancellationToken);
    Task<SchoolDto> UpdateAsync(Guid schoolId, UpdateSchoolRequestDto request, CancellationToken cancellationToken);
    Task<SchoolDto> GetByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SchoolDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<SchoolDto> SetActivationAsync(Guid schoolId, SetSchoolActivationRequestDto request, CancellationToken cancellationToken);
    Task<SchoolDto> ExtendSubscriptionAsync(Guid schoolId, ExtendSchoolSubscriptionRequestDto request, CancellationToken cancellationToken);
}
