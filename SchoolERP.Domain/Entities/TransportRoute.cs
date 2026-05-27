using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class TransportRoute : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid VehicleId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string PickupPoint { get; set; } = string.Empty;
    public string DropPoint { get; set; } = string.Empty;
    public TimeSpan PickupTime { get; set; }
    public TimeSpan DropTime { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public TransportVehicle? Vehicle { get; set; }
    public ICollection<StudentTransportAssignment> StudentAssignments { get; set; } = new List<StudentTransportAssignment>();
}
