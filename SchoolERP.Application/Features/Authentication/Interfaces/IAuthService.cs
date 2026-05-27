using SchoolERP.Application.Features.Authentication.Models;

namespace SchoolERP.Application.Features.Authentication.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress, CancellationToken cancellationToken);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress, CancellationToken cancellationToken);
    Task<AuthResponseDto> ChangePasswordAsync(ChangePasswordRequestDto request, string? ipAddress, CancellationToken cancellationToken);
}
