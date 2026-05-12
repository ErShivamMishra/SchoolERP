namespace SchoolERP.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? SchoolId { get; }
    Guid? TenantId { get; }
    string Role { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    bool IsPlatformUser { get; }
    bool IsFirstLogin { get; }
    bool RequiresPasswordReset { get; }
    IReadOnlyCollection<string> Roles { get; }
}
