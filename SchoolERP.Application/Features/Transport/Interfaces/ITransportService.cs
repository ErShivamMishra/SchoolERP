using SchoolERP.Application.Features.Transport.Models;

namespace SchoolERP.Application.Features.Transport.Interfaces;

public interface ITransportService
{
    Task<TransportVehicleDto> CreateVehicleAsync(CreateVehicleRequestDto request, CancellationToken cancellationToken);
    Task<TransportRouteDto> CreateRouteAsync(CreateRouteRequestDto request, CancellationToken cancellationToken);
    Task<TransportDriverDto> CreateDriverAsync(CreateDriverRequestDto request, CancellationToken cancellationToken);
    Task<StudentTransportAssignmentDto> AssignStudentAsync(AssignStudentTransportRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransportVehicleDto>> GetVehiclesAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransportRouteDto>> GetRoutesAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransportDriverDto>> GetDriversAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StudentTransportAssignmentDto>> GetAssignmentsAsync(Guid? schoolId, CancellationToken cancellationToken);
}
