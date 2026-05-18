using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Entities;

public sealed class UserPermission : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid ModuleId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }

    public User? User { get; set; }
    public Module? Module { get; set; }
}
