using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class FineRule : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GraceDays { get; set; }
    public decimal FlatAmount { get; set; }
    public decimal DailyAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
}
