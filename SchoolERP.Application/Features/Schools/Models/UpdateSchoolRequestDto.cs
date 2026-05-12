namespace SchoolERP.Application.Features.Schools.Models;

public sealed class UpdateSchoolRequestDto
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public int MaxStaffLimit { get; init; }
}
