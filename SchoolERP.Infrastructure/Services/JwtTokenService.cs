using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Options;

namespace SchoolERP.Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> jwtOptions) : IJwtTokenService
{
    public JwtTokenResult GenerateTokens(User user, string role, string? ipAddress)
    {
        var options = jwtOptions.Value;
        var now = DateTime.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(options.AccessTokenMinutes);
        var refreshTokenExpiresAt = now.AddDays(options.RefreshTokenDays);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("UserId", user.Id.ToString()),
            new("full_name", user.FullName),
            new("is_platform_user", user.IsPlatformUser.ToString()),
            new("force_password_reset", user.RequiresPasswordReset.ToString()),
            new("IsFirstLogin", user.IsFirstLogin.ToString()),
            new("Role", role)
        };

        if (user.SchoolId.HasValue)
        {
            claims.Add(new Claim("SchoolId", user.SchoolId.Value.ToString()));
            claims.Add(new Claim("school_id", user.SchoolId.Value.ToString()));
            claims.Add(new Claim("tenant_id", user.SchoolId.Value.ToString()));
        }

        claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiresAt,
            signingCredentials: credentials);

        return new JwtTokenResult
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
            RefreshToken = refreshToken,
            RefreshTokenHash = HashRefreshToken(refreshToken),
            AccessTokenExpiresAtUtc = accessTokenExpiresAt,
            RefreshTokenExpiresAtUtc = refreshTokenExpiresAt
        };
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
