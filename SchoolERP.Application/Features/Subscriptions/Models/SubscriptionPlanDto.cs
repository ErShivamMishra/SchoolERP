namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class SubscriptionPlanDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
}
