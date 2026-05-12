namespace SchoolERP.Application.Features.Authentication.Models;

public sealed class RefreshTokenRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
