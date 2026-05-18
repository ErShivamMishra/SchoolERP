namespace SchoolERP.Application.Features.AccessControl.Models;

public sealed class UpsertUserModulePermissionDto
{
    public Guid ModuleId { get; init; }
    public bool CanView { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
}
