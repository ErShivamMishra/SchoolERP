namespace SchoolERP.Application.Common.Models;

public sealed class ModulePermissionDto
{
    public Guid ModuleId { get; init; }
    public string ModuleName { get; init; } = string.Empty;
    public string ModuleCode { get; init; } = string.Empty;
    public bool CanView { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
}
