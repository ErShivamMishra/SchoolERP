namespace SchoolERP.Application.Features.Schools.Models;

public sealed class CreateSchoolRequestDto
{
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public DateTime SubscriptionStartDate { get; init; }
    public DateTime SubscriptionEndDate { get; init; }
    public int MaxStaffLimit { get; init; }
}
