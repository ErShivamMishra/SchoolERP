using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Common.Responses;
using SchoolERP.Application.Features.Authentication.Interfaces;
using SchoolERP.Application.Features.Authentication.Models;

namespace SchoolERP.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Authentication completed successfully."));
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var response = await authService.RefreshTokenAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok(ApiResponseFactory.Success(response, "Refresh token processed successfully."));
    }
}
