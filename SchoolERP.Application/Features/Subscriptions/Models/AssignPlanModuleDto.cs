namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class AssignPlanModuleDto
{
    public Guid ModuleId { get; init; }
    public bool IsLicensed { get; init; }
    public bool IsVisibleInMenu { get; init; } = true;
}
