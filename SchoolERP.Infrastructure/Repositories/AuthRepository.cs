using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Domain.Entities;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Repositories;

public sealed class AuthRepository(SchoolErpDbContext dbContext) : IAuthRepository
{
    public async Task<User?> GetUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        return await dbContext.RefreshTokens
            .Include(x => x.User)
            .ThenInclude(x => x!.Role)
            .FirstOrDefaultAsync(x => x.Token == refreshTokenHash, cancellationToken);
    }

    public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        return dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
