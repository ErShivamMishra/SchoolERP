namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class PlanModuleDto
{
    public Guid ModuleId { get; init; }
    public string ModuleName { get; init; } = string.Empty;
    public string ModuleCode { get; init; } = string.Empty;
    public bool IsLicensed { get; init; }
    public bool IsVisibleInMenu { get; init; }
}
