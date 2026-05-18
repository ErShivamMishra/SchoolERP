namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class AssignSchoolPlanRequestDto
{
    public Guid SubscriptionPlanId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
