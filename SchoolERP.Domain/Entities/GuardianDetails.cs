using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class GuardianDetails : AuditableEntity
{
    public Guid SchoolId { get; set; }
    public Guid AdmissionId { get; set; }
    public string GuardianName { get; set; } = string.Empty;
    public string GuardianPhone { get; set; } = string.Empty;
    public string GuardianRelation { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }

    public School? School { get; set; }
    public Admission? Admission { get; set; }
}
