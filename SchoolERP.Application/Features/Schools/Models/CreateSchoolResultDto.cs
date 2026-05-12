namespace SchoolERP.Application.Features.Schools.Models;

public sealed class CreateSchoolResultDto
{
    public SchoolDto School { get; init; } = new();
    public string SchoolAdminEmail { get; init; } = string.Empty;
    public string TemporaryPassword { get; init; } = string.Empty;
}
