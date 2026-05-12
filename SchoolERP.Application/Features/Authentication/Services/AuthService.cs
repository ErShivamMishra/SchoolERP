using FluentValidation;
using SchoolERP.Application.Common.Exceptions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.Models;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.Authentication.Models;
using SchoolERP.Domain.Constants;
using SchoolERP.Domain.Entities;
using SchoolERP.Domain.Enums;

namespace SchoolERP.Application.Features.Authentication.Services;

public sealed class AuthService(
    IAuthRepository authRepository,
    IJwtTokenService jwtTokenService,
    IPasswordHasher passwordHasher,
    IAuditService auditService,
    IValidator<LoginRequestDto> loginValidator,
    IValidator<RefreshTokenRequestDto> refreshTokenValidator,
    IAuthPolicyProvider authPolicyProvider) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await authRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            await auditService.WriteAsync("Authentication", "LoginFailed", nameof(User), null, "Denied", $"Unknown email: {request.Email.Trim()}", null, null, cancellationToken);
            throw new UnauthorizedException("Invalid email or password.", "invalid_credentials");
        }

        EnsureUserHasRole(user);

        if (!user.IsActive)
        {
            await auditService.WriteAsync("Authentication", "LoginFailed", nameof(User), user.Id.ToString(), "Denied", "Inactive user attempted login.", user.SchoolId, user.Id, cancellationToken);
            throw new ForbiddenException("User account is inactive.", "user_inactive");
        }

        if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value > DateTime.UtcNow)
        {
            await auditService.WriteAsync("Authentication", "LoginBlocked", nameof(User), user.Id.ToString(), "Denied", "Account is locked.", user.SchoolId, user.Id, cancellationToken);
            throw new UnauthorizedException("Account is locked. Please try again later.", "account_locked");
        }

        if (!passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            await RegisterFailedLoginAttemptAsync(user, ipAddress, cancellationToken);
            throw new UnauthorizedException("Invalid email or password.", "invalid_credentials");
        }

        await EnsureSubscriptionAllowsAuthenticationAsync(user, cancellationToken);

        user.FailedLoginAttempts = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAtUtc = DateTime.UtcNow;

        var response = await IssueTokensAsync(user, ipAddress, cancellationToken);

        await auditService.WriteAsync("Authentication", "LoginSucceeded", nameof(User), user.Id.ToString(), "Success", $"Role: {user.Role!.Name}", user.SchoolId, user.Id, cancellationToken);

        return response;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress, CancellationToken cancellationToken)
    {
        await refreshTokenValidator.ValidateAndThrowAsync(request, cancellationToken);

        var refreshTokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken.Trim());
        var persistedToken = await authRepository.GetRefreshTokenAsync(refreshTokenHash, cancellationToken);

        if (persistedToken is null || persistedToken.User is null || !persistedToken.IsActive)
        {
            throw new UnauthorizedException("Refresh token is invalid or expired.", "invalid_refresh_token");
        }

        EnsureUserHasRole(persistedToken.User);

        if (!persistedToken.User.IsActive)
        {
            throw new ForbiddenException("User account is inactive.", "user_inactive");
        }

        await EnsureSubscriptionAllowsAuthenticationAsync(persistedToken.User, cancellationToken);

        var response = await RotateRefreshTokenAsync(persistedToken, ipAddress, cancellationToken);

        await auditService.WriteAsync("Authentication", "TokenRefreshed", nameof(User), persistedToken.User.Id.ToString(), "Success", $"Role: {persistedToken.User.Role!.Name}", persistedToken.User.SchoolId, persistedToken.User.Id, cancellationToken);

        return response;
    }

    private async Task<AuthResponseDto> RotateRefreshTokenAsync(RefreshToken persistedToken, string? ipAddress, CancellationToken cancellationToken)
    {
        var tokenResult = jwtTokenService.GenerateTokens(persistedToken.User!, persistedToken.User!.Role!.Name, ipAddress);

        persistedToken.RevokedAtUtc = DateTime.UtcNow;
        persistedToken.RevokedByIp = ipAddress;
        persistedToken.ReplacedByToken = tokenResult.RefreshTokenHash;
        persistedToken.ReasonRevoked = "Rotated";

        await authRepository.AddRefreshTokenAsync(CreateRefreshTokenEntity(persistedToken.User!, tokenResult, ipAddress), cancellationToken);
        persistedToken.User!.LastLoginAtUtc = DateTime.UtcNow;
        await authRepository.SaveChangesAsync(cancellationToken);

        return MapResponse(persistedToken.User!, tokenResult);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, string? ipAddress, CancellationToken cancellationToken)
    {
        var tokenResult = jwtTokenService.GenerateTokens(user, user.Role!.Name, ipAddress);
        await authRepository.AddRefreshTokenAsync(CreateRefreshTokenEntity(user, tokenResult, ipAddress), cancellationToken);
        await authRepository.SaveChangesAsync(cancellationToken);
        return MapResponse(user, tokenResult);
    }

    private static RefreshToken CreateRefreshTokenEntity(User user, JwtTokenResult tokenResult, string? ipAddress)
    {
        return new RefreshToken
        {
            UserId = user.Id,
            Token = tokenResult.RefreshTokenHash,
            ExpiresAtUtc = tokenResult.RefreshTokenExpiresAtUtc,
            CreatedByIp = ipAddress
        };
    }

    private async Task RegisterFailedLoginAttemptAsync(User user, string? ipAddress, CancellationToken cancellationToken)
    {
        user.FailedLoginAttempts += 1;

        var threshold = authPolicyProvider.GetFailedLoginThreshold();
        var isLocked = user.FailedLoginAttempts >= threshold;

        if (isLocked)
        {
            user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(authPolicyProvider.GetLockoutDurationMinutes());
        }

        await authRepository.SaveChangesAsync(cancellationToken);

        var action = isLocked ? "AccountLocked" : "LoginFailed";
        var detail = isLocked
            ? $"Failed attempts reached threshold from IP {ipAddress ?? "unknown"}."
            : $"Failed login attempt {user.FailedLoginAttempts} from IP {ipAddress ?? "unknown"}.";

        await auditService.WriteAsync("Authentication", action, nameof(User), user.Id.ToString(), "Denied", detail, user.SchoolId, user.Id, cancellationToken);
    }

    private async Task EnsureSubscriptionAllowsAuthenticationAsync(User user, CancellationToken cancellationToken)
    {
        if (user.Role!.Name.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!user.SchoolId.HasValue)
        {
            throw new ForbiddenException("School context is required for tenant users.", "school_context_required");
        }

        var subscriptionSnapshot = await authPolicyProvider.GetSubscriptionSnapshotAsync(user.SchoolId.Value, cancellationToken);
        if (subscriptionSnapshot is null)
        {
            throw new ForbiddenException("No active subscription found for this school.", "subscription_required");
        }

        if (subscriptionSnapshot.Status == SubscriptionStatus.Suspended)
        {
            throw new ForbiddenException("School subscription is suspended.", "subscription_suspended");
        }

        if (subscriptionSnapshot.Status == SubscriptionStatus.Expired &&
            (!subscriptionSnapshot.GracePeriodEndDateUtc.HasValue || subscriptionSnapshot.GracePeriodEndDateUtc.Value < DateTime.UtcNow))
        {
            throw new ForbiddenException("School subscription is expired.", "subscription_expired");
        }
    }

    private static AuthResponseDto MapResponse(User user, JwtTokenResult tokenResult)
    {
        return new AuthResponseDto
        {
            AccessToken = tokenResult.AccessToken,
            AccessTokenExpiresAtUtc = tokenResult.AccessTokenExpiresAtUtc,
            RefreshToken = tokenResult.RefreshToken,
            RefreshTokenExpiresAtUtc = tokenResult.RefreshTokenExpiresAtUtc,
            User = new AuthUserDto
            {
                Id = user.Id,
                SchoolId = user.SchoolId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role!.Name,
                IsFirstLogin = user.IsFirstLogin
            }
        };
    }

    private static void EnsureUserHasRole(User user)
    {
        if (user.Role is null)
        {
            throw new ForbiddenException("User role is not assigned.", "role_not_assigned");
        }
    }
}
