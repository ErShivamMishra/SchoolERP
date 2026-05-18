using SchoolERP.Application.Features.Subscriptions.Models;

namespace SchoolERP.Application.Features.Subscriptions.Interfaces;

public interface ISubscriptionPlanService
{
    Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PlanModuleDto>> AssignModulesAsync(Guid planId, AssignPlanModulesRequestDto request, CancellationToken cancellationToken);
    Task<SchoolSubscriptionDto> AssignPlanToSchoolAsync(Guid schoolId, AssignSchoolPlanRequestDto request, CancellationToken cancellationToken);
}
