using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PhysioDesk.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "physiodesk_refresh";
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password, ct);
        if (!result.Succeeded)
        {
            return Problem(statusCode: 401, title: result.Error);
        }

        SetRefreshCookie(result.RawRefreshToken!, result.RefreshTokenExpiresAtUtc);
        return Ok(ToResponse(result));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var rawToken) || string.IsNullOrEmpty(rawToken))
        {
            return Problem(statusCode: 401, title: "No refresh token supplied.");
        }

        var result = await _authService.RefreshAsync(rawToken, ct);
        if (!result.Succeeded)
        {
            Response.Cookies.Delete(RefreshCookieName);
            return Problem(statusCode: 401, title: result.Error);
        }

        SetRefreshCookie(result.RawRefreshToken!, result.RefreshTokenExpiresAtUtc);
        return Ok(ToResponse(result));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        await _authService.LogoutAsync(userId, ct);
        Response.Cookies.Delete(RefreshCookieName);
        return NoContent();
    }

    private void SetRefreshCookie(string rawToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(RefreshCookieName, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAtUtc,
            Path = "/api/auth"
        });
    }

    private static LoginResponse ToResponse(AuthResult result) => new(
        result.AccessToken!,
        result.AccessTokenExpiresAtUtc,
        new UserSummary(result.User!.Id, result.User.Email, result.User.DisplayName, result.User.Role.ToString()));
}
