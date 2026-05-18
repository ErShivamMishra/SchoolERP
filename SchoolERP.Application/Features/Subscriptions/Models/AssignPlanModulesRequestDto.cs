namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class AssignPlanModulesRequestDto
{
    public IReadOnlyCollection<AssignPlanModuleDto> Modules { get; init; } = Array.Empty<AssignPlanModuleDto>();
}
