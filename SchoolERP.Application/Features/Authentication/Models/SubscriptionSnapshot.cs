using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Authentication.Models;

public sealed class SubscriptionSnapshot
{
    public SubscriptionStatus Status { get; init; }
    public DateTime EndDateUtc { get; init; }
    public DateTime? GracePeriodEndDateUtc { get; init; }
}
