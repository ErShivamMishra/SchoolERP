using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.Interfaces;
using SchoolERP.Application.Features.Transport.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Transport.Services;

public sealed class TransportService(
    ITransportRepository repository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IValidator<CreateVehicleRequestDto> vehicleValidator,
    IValidator<CreateRouteRequestDto> routeValidator,
    IValidator<CreateDriverRequestDto> driverValidator,
    IValidator<AssignStudentTransportRequestDto> assignmentValidator) : ITransportService
{
    public async Task<TransportVehicleDto> CreateVehicleAsync(CreateVehicleRequestDto request, CancellationToken cancellationToken)
    {
        await vehicleValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var vehicle = new TransportVehicle
        {
            SchoolId = schoolId,
            VehicleNumber = request.VehicleNumber.Trim(),
            VehicleType = request.VehicleType.Trim(),
            Capacity = request.Capacity,
            CreatedBy = currentUserContext.UserId?.ToString()
        };
        await repository.AddVehicleAsync(vehicle, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync(ModuleCodes.TransportManagement, "VehicleCreated", nameof(TransportVehicle), vehicle.Id.ToString(), "Success", "Vehicle created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapVehicle(vehicle);
    }

    public async Task<TransportRouteDto> CreateRouteAsync(CreateRouteRequestDto request, CancellationToken cancellationToken)
    {
        await routeValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var vehicle = await repository.GetVehicleByIdAsync(request.VehicleId, cancellationToken) ?? throw new NotFoundException("Vehicle not found.", "vehicle_not_found");
        EnsureSchoolOwned(schoolId, vehicle.SchoolId);
        var route = new TransportRoute
        {
            SchoolId = schoolId,
            VehicleId = request.VehicleId,
            RouteName = request.RouteName.Trim(),
            PickupPoint = request.PickupPoint.Trim(),
            DropPoint = request.DropPoint.Trim(),
            PickupTime = request.PickupTime,
            DropTime = request.DropTime,
            CreatedBy = currentUserContext.UserId?.ToString()
        };
        await repository.AddRouteAsync(route, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        route.Vehicle = vehicle;
        await auditService.WriteAsync(ModuleCodes.TransportManagement, "RouteCreated", nameof(TransportRoute), route.Id.ToString(), "Success", "Route created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapRoute(route);
    }

    public async Task<TransportDriverDto> CreateDriverAsync(CreateDriverRequestDto request, CancellationToken cancellationToken)
    {
        await driverValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        TransportVehicle? vehicle = null;
        if (request.VehicleId.HasValue)
        {
            vehicle = await repository.GetVehicleByIdAsync(request.VehicleId.Value, cancellationToken) ?? throw new NotFoundException("Vehicle not found.", "vehicle_not_found");
            EnsureSchoolOwned(schoolId, vehicle.SchoolId);
        }
        var driver = new TransportDriver
        {
            SchoolId = schoolId,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            LicenseNumber = request.LicenseNumber.Trim(),
            VehicleId = request.VehicleId,
            CreatedBy = currentUserContext.UserId?.ToString()
        };
        await repository.AddDriverAsync(driver, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        driver.Vehicle = vehicle;
        await auditService.WriteAsync(ModuleCodes.TransportManagement, "DriverCreated", nameof(TransportDriver), driver.Id.ToString(), "Success", "Driver created.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapDriver(driver);
    }

    public async Task<StudentTransportAssignmentDto> AssignStudentAsync(AssignStudentTransportRequestDto request, CancellationToken cancellationToken)
    {
        await assignmentValidator.ValidateAndThrowAsync(request, cancellationToken);
        var schoolId = await ResolveSchoolIdAsync(request.SchoolId, cancellationToken);
        var student = await repository.GetStudentByIdAsync(request.StudentId, cancellationToken) ?? throw new NotFoundException("Student not found.", "student_not_found");
        var route = await repository.GetRouteByIdAsync(request.RouteId, cancellationToken) ?? throw new NotFoundException("Route not found.", "route_not_found");
        EnsureSchoolOwned(schoolId, student.SchoolId);
        EnsureSchoolOwned(schoolId, route.SchoolId);
        var assignment = await repository.GetAssignmentByStudentIdAsync(schoolId, request.StudentId, cancellationToken) ?? new StudentTransportAssignment
        {
            SchoolId = schoolId,
            StudentId = request.StudentId,
            CreatedBy = currentUserContext.UserId?.ToString()
        };
        assignment.RouteId = request.RouteId;
        assignment.PickupLocation = request.PickupLocation.Trim();
        assignment.DropLocation = request.DropLocation.Trim();
        assignment.GuardianContactNumber = request.GuardianContactNumber?.Trim();
        assignment.ModifiedAtUtc = DateTime.UtcNow;
        assignment.ModifiedBy = currentUserContext.UserId?.ToString();
        await repository.AddOrUpdateAssignmentAsync(assignment, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        assignment.Student = student;
        assignment.Route = route;
        await auditService.WriteAsync(ModuleCodes.TransportManagement, "StudentTransportAssigned", nameof(StudentTransportAssignment), assignment.Id.ToString(), "Success", "Student assigned to transport route.", schoolId, currentUserContext.UserId, cancellationToken);
        return MapAssignment(assignment);
    }

    public async Task<IReadOnlyCollection<TransportVehicleDto>> GetVehiclesAsync(Guid? schoolId, CancellationToken cancellationToken)
        => (await repository.GetVehiclesAsync(ResolveSchoolIdForRead(schoolId), cancellationToken)).Select(MapVehicle).ToArray();

    public async Task<IReadOnlyCollection<TransportRouteDto>> GetRoutesAsync(Guid? schoolId, CancellationToken cancellationToken)
        => (await repository.GetRoutesAsync(ResolveSchoolIdForRead(schoolId), cancellationToken)).Select(MapRoute).ToArray();

    public async Task<IReadOnlyCollection<TransportDriverDto>> GetDriversAsync(Guid? schoolId, CancellationToken cancellationToken)
        => (await repository.GetDriversAsync(ResolveSchoolIdForRead(schoolId), cancellationToken)).Select(MapDriver).ToArray();

    public async Task<IReadOnlyCollection<StudentTransportAssignmentDto>> GetAssignmentsAsync(Guid? schoolId, CancellationToken cancellationToken)
        => (await repository.GetAssignmentsAsync(ResolveSchoolIdForRead(schoolId), cancellationToken)).Select(MapAssignment).ToArray();

    private async Task<Guid> ResolveSchoolIdAsync(Guid? requestedSchoolId, CancellationToken cancellationToken)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            var schoolId = requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
            _ = await repository.GetSchoolByIdAsync(schoolId, cancellationToken) ?? throw new NotFoundException("School not found.", "school_not_found");
            return schoolId;
        }
        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }
        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Transport access is limited to the current school.", "cross_tenant_access_forbidden");
        }
        return currentUserContext.SchoolId.Value;
    }

    private Guid ResolveSchoolIdForRead(Guid? requestedSchoolId)
    {
        if (currentUserContext.Roles.Contains(RoleNames.SuperAdmin))
        {
            return requestedSchoolId ?? throw new BadRequestException("SchoolId is required for SuperAdmin requests.", "school_id_required");
        }
        if (!currentUserContext.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for this request.", "school_context_required");
        }
        if (requestedSchoolId.HasValue && requestedSchoolId.Value != currentUserContext.SchoolId.Value)
        {
            throw new ForbiddenException("Transport access is limited to the current school.", "cross_tenant_access_forbidden");
        }
        return currentUserContext.SchoolId.Value;
    }

    private static void EnsureSchoolOwned(Guid expected, Guid actual)
    {
        if (expected != actual)
        {
            throw new ForbiddenException("Transport access is limited to the current school.", "cross_tenant_access_forbidden");
        }
    }

    private static TransportVehicleDto MapVehicle(TransportVehicle vehicle) => new()
    {
        Id = vehicle.Id,
        VehicleNumber = vehicle.VehicleNumber,
        VehicleType = vehicle.VehicleType,
        Capacity = vehicle.Capacity,
        IsActive = vehicle.IsActive
    };

    private static TransportRouteDto MapRoute(TransportRoute route) => new()
    {
        Id = route.Id,
        VehicleId = route.VehicleId,
        VehicleNumber = route.Vehicle?.VehicleNumber ?? string.Empty,
        RouteName = route.RouteName,
        PickupPoint = route.PickupPoint,
        DropPoint = route.DropPoint,
        PickupTime = route.PickupTime,
        DropTime = route.DropTime,
        IsActive = route.IsActive
    };

    private static TransportDriverDto MapDriver(TransportDriver driver) => new()
    {
        Id = driver.Id,
        FullName = driver.FullName,
        PhoneNumber = driver.PhoneNumber,
        LicenseNumber = driver.LicenseNumber,
        VehicleId = driver.VehicleId,
        VehicleNumber = driver.Vehicle?.VehicleNumber,
        IsActive = driver.IsActive
    };

    private static StudentTransportAssignmentDto MapAssignment(StudentTransportAssignment assignment) => new()
    {
        Id = assignment.Id,
        StudentId = assignment.StudentId,
        StudentName = assignment.Student is null ? string.Empty : $"{assignment.Student.FirstName} {assignment.Student.LastName}".Trim(),
        RollNumber = assignment.Student?.RollNumber ?? string.Empty,
        RouteId = assignment.RouteId,
        RouteName = assignment.Route?.RouteName ?? string.Empty,
        PickupLocation = assignment.PickupLocation,
        DropLocation = assignment.DropLocation,
        GuardianContactNumber = assignment.GuardianContactNumber,
        IsActive = assignment.IsActive
    };
}
