using SchoolERP.Application.Features.Authentication.Models;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Authentication.Interfaces;

public interface IAuthPolicyProvider
{
    int GetFailedLoginThreshold();
    int GetLockoutDurationMinutes();
    Task<SubscriptionSnapshot?> GetSubscriptionSnapshotAsync(Guid schoolId, CancellationToken cancellationToken);
}
