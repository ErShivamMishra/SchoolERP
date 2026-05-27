using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class TransportVehicle : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public ICollection<TransportRoute> Routes { get; set; } = new List<TransportRoute>();
}
