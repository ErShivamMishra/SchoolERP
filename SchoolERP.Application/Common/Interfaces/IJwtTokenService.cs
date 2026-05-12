using SchoolERP.Application.Common.Models;
using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Common.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateTokens(User user, string role, string? ipAddress);
    string HashRefreshToken(string refreshToken);
}
