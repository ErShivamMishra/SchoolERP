namespace SchoolERP.Application.Features.Authentication.Models;

public sealed class AuthUserDto
{
    public Guid Id { get; init; }
    public Guid? SchoolId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsFirstLogin { get; init; }
}
