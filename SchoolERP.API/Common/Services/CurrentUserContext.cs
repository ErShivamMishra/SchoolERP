using System.Security.Claims;
using SchoolERP.Application.Common.Interfaces;

namespace SchoolERP.API.Common.Services;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId => TryGetGuid("UserId") ?? TryGetGuid(ClaimTypes.NameIdentifier) ?? TryGetGuid("sub");
    public Guid? SchoolId => TryGetGuid("SchoolId") ?? TryGetGuid("school_id") ?? TryGetGuid("tenant_id");
    public Guid? TenantId => SchoolId;
    public string Role => User?.FindFirstValue("Role") ?? User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    public string Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email") ?? string.Empty;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public bool IsPlatformUser => bool.TryParse(User?.FindFirstValue("is_platform_user"), out var value) && value;
    public bool IsFirstLogin => bool.TryParse(User?.FindFirstValue("IsFirstLogin"), out var value) && value;
    public bool RequiresPasswordReset => bool.TryParse(User?.FindFirstValue("force_password_reset"), out var value) && value;
    public IReadOnlyCollection<string> Roles => User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray() ?? Array.Empty<string>();

    private Guid? TryGetGuid(string claimType)
    {
        var value = User?.FindFirstValue(claimType);
        return Guid.TryParse(value, out var result) ? result : null;
    }
}
