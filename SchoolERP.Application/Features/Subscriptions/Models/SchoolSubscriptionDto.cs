namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class SchoolSubscriptionDto
{
    public Guid SchoolId { get; init; }
    public Guid SubscriptionPlanId { get; init; }
    public string SubscriptionPlanCode { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
