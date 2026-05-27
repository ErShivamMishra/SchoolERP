using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class StudentTransportAssignment : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public Guid RouteId { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DropLocation { get; set; } = string.Empty;
    public string? GuardianContactNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Student? Student { get; set; }
    public TransportRoute? Route { get; set; }
}
