using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Transport.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class TransportRepository(SchoolErpDbContext dbContext) : ITransportRepository
{
    public Task<School?> GetSchoolByIdAsync(Guid schoolId, CancellationToken cancellationToken)
        => dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);
    public Task<TransportVehicle?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken cancellationToken)
        => dbContext.TransportVehicles.FirstOrDefaultAsync(x => x.Id == vehicleId, cancellationToken);
    public Task<TransportRoute?> GetRouteByIdAsync(Guid routeId, CancellationToken cancellationToken)
        => dbContext.TransportRoutes.Include(x => x.Vehicle).FirstOrDefaultAsync(x => x.Id == routeId, cancellationToken);
    public Task<Student?> GetStudentByIdAsync(Guid studentId, CancellationToken cancellationToken)
        => dbContext.Students.FirstOrDefaultAsync(x => x.Id == studentId, cancellationToken);
    public Task AddVehicleAsync(TransportVehicle vehicle, CancellationToken cancellationToken)
        => dbContext.TransportVehicles.AddAsync(vehicle, cancellationToken).AsTask();
    public Task AddRouteAsync(TransportRoute route, CancellationToken cancellationToken)
        => dbContext.TransportRoutes.AddAsync(route, cancellationToken).AsTask();
    public Task AddDriverAsync(TransportDriver driver, CancellationToken cancellationToken)
        => dbContext.TransportDrivers.AddAsync(driver, cancellationToken).AsTask();
    public Task AddOrUpdateAssignmentAsync(StudentTransportAssignment assignment, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(assignment).State == EntityState.Detached)
        {
            dbContext.StudentTransportAssignments.Update(assignment);
        }
        return Task.CompletedTask;
    }
    public async Task<IReadOnlyCollection<TransportVehicle>> GetVehiclesAsync(Guid schoolId, CancellationToken cancellationToken)
        => await dbContext.TransportVehicles.Where(x => x.SchoolId == schoolId).OrderBy(x => x.VehicleNumber).ToListAsync(cancellationToken);
    public async Task<IReadOnlyCollection<TransportRoute>> GetRoutesAsync(Guid schoolId, CancellationToken cancellationToken)
        => await dbContext.TransportRoutes.Include(x => x.Vehicle).Where(x => x.SchoolId == schoolId).OrderBy(x => x.RouteName).ToListAsync(cancellationToken);
    public async Task<IReadOnlyCollection<TransportDriver>> GetDriversAsync(Guid schoolId, CancellationToken cancellationToken)
        => await dbContext.TransportDrivers.Include(x => x.Vehicle).Where(x => x.SchoolId == schoolId).OrderBy(x => x.FullName).ToListAsync(cancellationToken);
    public async Task<IReadOnlyCollection<StudentTransportAssignment>> GetAssignmentsAsync(Guid schoolId, CancellationToken cancellationToken)
        => await dbContext.StudentTransportAssignments.Include(x => x.Student).Include(x => x.Route).Where(x => x.SchoolId == schoolId).OrderBy(x => x.Student!.RollNumber).ToListAsync(cancellationToken);
    public Task<StudentTransportAssignment?> GetAssignmentByStudentIdAsync(Guid schoolId, Guid studentId, CancellationToken cancellationToken)
        => dbContext.StudentTransportAssignments.Include(x => x.Student).Include(x => x.Route).FirstOrDefaultAsync(x => x.SchoolId == schoolId && x.StudentId == studentId, cancellationToken);
    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);
}
