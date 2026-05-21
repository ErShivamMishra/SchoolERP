using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class FeeCategory : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public ICollection<FeeStructure> FeeStructures { get; set; } = new List<FeeStructure>();
}
