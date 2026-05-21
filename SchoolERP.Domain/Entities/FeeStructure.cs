using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class FeeStructure : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid FeeCategoryId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime EffectiveFromDate { get; set; }
    public DateTime? EffectiveToDate { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public FeeCategory? FeeCategory { get; set; }
    public Class? Class { get; set; }
    public Section? Section { get; set; }
    public ICollection<StudentFeeAssignment> StudentFeeAssignments { get; set; } = new List<StudentFeeAssignment>();
}
