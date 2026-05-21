using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class StudentFeeAssignment : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }
    public Guid? AcademicSessionId { get; set; }
    public decimal AssignedAmount { get; set; }
    public DateTime AssignedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[]? RowVersion { get; set; }

    public School? School { get; set; }
    public Student? Student { get; set; }
    public FeeStructure? FeeStructure { get; set; }
    public AcademicSession? AcademicSession { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
