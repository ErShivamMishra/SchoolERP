using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Transport.Interfaces;

public interface ITransportRepository
{
    Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<TransportVehicle?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<TransportRoute?> GetRouteByIdAsync(Guid routeId, CancellationToken cancellationToken);
    Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken);
    Task AddVehicleAsync(TransportVehicle vehicle, CancellationToken cancellationToken);
    Task AddRouteAsync(TransportRoute route, CancellationToken cancellationToken);
    Task AddDriverAsync(TransportDriver driver, CancellationToken cancellationToken);
    Task AddOrUpdateAssignmentAsync(StudentTransportAssignment assignment, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransportVehicle>> GetVehiclesAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransportRoute>> GetRoutesAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransportDriver>> GetDriversAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StudentTransportAssignment>> GetAssignmentsAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<StudentTransportAssignment?> GetAssignmentByStudentIdAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
