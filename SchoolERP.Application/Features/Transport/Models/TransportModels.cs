using FluentValidation;
using SchoolERP.Application.Common.Models;

namespace SchoolERP.Application.Features.Transport.Models;

public sealed class CreateVehicleRequestDto
{
    public Guid? SchoolId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public int Capacity { get; init; }
}

public sealed class CreateRouteRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid VehicleId { get; init; }
    public string RouteName { get; init; } = string.Empty;
    public string PickupPoint { get; init; } = string.Empty;
    public string DropPoint { get; init; } = string.Empty;
    public TimeSpan PickupTime { get; init; }
    public TimeSpan DropTime { get; init; }
}

public sealed class CreateDriverRequestDto
{
    public Guid? SchoolId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string LicenseNumber { get; init; } = string.Empty;
    public Guid? VehicleId { get; init; }
}

public sealed class AssignStudentTransportRequestDto
{
    public Guid? SchoolId { get; init; }
    public Guid StudentId { get; init; }
    public Guid RouteId { get; init; }
    public string PickupLocation { get; init; } = string.Empty;
    public string DropLocation { get; init; } = string.Empty;
    public string? GuardianContactNumber { get; init; }
}

public sealed class TransportVehicleDto
{
    public Guid Id { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public bool IsActive { get; init; }
}

public sealed class TransportRouteDto
{
    public Guid Id { get; init; }
    public Guid VehicleId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public string RouteName { get; init; } = string.Empty;
    public string PickupPoint { get; init; } = string.Empty;
    public string DropPoint { get; init; } = string.Empty;
    public TimeSpan PickupTime { get; init; }
    public TimeSpan DropTime { get; init; }
    public bool IsActive { get; init; }
}

public sealed class TransportDriverDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string LicenseNumber { get; init; } = string.Empty;
    public Guid? VehicleId { get; init; }
    public string? VehicleNumber { get; init; }
    public bool IsActive { get; init; }
}

public sealed class StudentTransportAssignmentDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
    public Guid RouteId { get; init; }
    public string RouteName { get; init; } = string.Empty;
    public string PickupLocation { get; init; } = string.Empty;
    public string DropLocation { get; init; } = string.Empty;
    public string? GuardianContactNumber { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CreateVehicleRequestDtoValidator : AbstractValidator<CreateVehicleRequestDto>
{
    public CreateVehicleRequestDtoValidator()
    {
        RuleFor(x => x.VehicleNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VehicleType).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}

public sealed class CreateRouteRequestDtoValidator : AbstractValidator<CreateRouteRequestDto>
{
    public CreateRouteRequestDtoValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.RouteName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.PickupPoint).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DropPoint).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateDriverRequestDtoValidator : AbstractValidator<CreateDriverRequestDto>
{
    public CreateDriverRequestDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
    }
}

public sealed class AssignStudentTransportRequestDtoValidator : AbstractValidator<AssignStudentTransportRequestDto>
{
    public AssignStudentTransportRequestDtoValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.RouteId).NotEmpty();
        RuleFor(x => x.PickupLocation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DropLocation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GuardianContactNumber).MaximumLength(30);
    }
}
