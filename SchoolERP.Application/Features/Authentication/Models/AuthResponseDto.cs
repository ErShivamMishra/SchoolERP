namespace SchoolERP.Application.Features.Authentication.Models;

public sealed class AuthResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
    public AuthUserDto User { get; init; } = new();
}
