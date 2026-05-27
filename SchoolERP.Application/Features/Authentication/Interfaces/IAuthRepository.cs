using SchoolERP.Domain.Entities;

namespace SchoolERP.Application.Features.Authentication.Interfaces;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
