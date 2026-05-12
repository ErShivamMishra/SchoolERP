using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.Authentication.Models;
using SchoolERP.Infrastructure.Options;
using SchoolERP.Infrastructure.Persistence;

namespace SchoolERP.Infrastructure.Services;

public sealed class AuthPolicyProvider(
    IOptions<AuthOptions> authOptions,
    SchoolErpDbContext dbContext) : IAuthPolicyProvider
{
    public int GetFailedLoginThreshold() => Math.Max(1, authOptions.Value.FailedLoginThreshold);

    public int GetLockoutDurationMinutes() => Math.Max(1, authOptions.Value.LockoutDurationMinutes);

    public async Task<SubscriptionSnapshot?> GetSubscriptionSnapshotAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var school = await dbContext.Schools.FirstOrDefaultAsync(x => x.Id == schoolId, cancellationToken);
        if (school is null)
        {
            return null;
        }

        var status = !school.IsActive || school.Status == Domain.Enums.SchoolStatus.Suspended
            ? Domain.Enums.SubscriptionStatus.Suspended
            : school.SubscriptionEndDateUtc < DateTime.UtcNow
                ? Domain.Enums.SubscriptionStatus.Expired
                : Domain.Enums.SubscriptionStatus.Active;

        return new SubscriptionSnapshot
        {
            Status = status,
            EndDateUtc = school.SubscriptionEndDateUtc,
            GracePeriodEndDateUtc = null
        };
    }
}
