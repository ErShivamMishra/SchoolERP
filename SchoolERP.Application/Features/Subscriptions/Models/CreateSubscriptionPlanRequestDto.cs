namespace SchoolERP.Application.Features.Subscriptions.Models;

public sealed class CreateSubscriptionPlanRequestDto
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
