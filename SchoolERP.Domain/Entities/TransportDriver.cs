using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class TransportDriver : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public TransportVehicle? Vehicle { get; set; }
}
