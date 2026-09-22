using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BareBonesApi.DTOs.Auth;
using BareBonesApi.Interfaces.Auth;

namespace BareBonesApi.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        await _authService.RegisterAsync(request);
        return Created("", new { Message = "Registration submitted successfully. Your account is pending admin approval." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var (user, accessToken, refreshToken, expiresAt) = await _authService.LoginAsync(request);

        SetAuthCookies(accessToken, refreshToken, expiresAt);

        return Ok(new { Message = "Login successful", User = user });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var token = Request.Cookies["refresh_token"];
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { Message = "Refresh token is required" });

        var (newAccessToken, newRefreshToken, expiresAt) = await _authService.RefreshTokenAsync(token);

        SetAuthCookies(newAccessToken, newRefreshToken, expiresAt);

        return Ok(new { Message = "Token refreshed successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { Message = "If that email exists, reset instructions have been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(new { Message = "Password reset successfully" });
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        // Extract the UserId from the JWT Claims
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized(new { Message = "Invalid user token" });

        await _authService.ChangePasswordAsync(userId, request);
        return Ok(new { Message = "Password changed successfully" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies["refresh_token"];
        if (!string.IsNullOrWhiteSpace(token))
        {
            await _authService.LogoutAsync(token);
        }

        ClearAuthCookies();
        return Ok(new { Message = "Logged out successfully" });
    }

    // Private Cookie Helpers

    private void SetAuthCookies(string accessToken, string refreshToken, DateTime refreshTokenExpiresAt)
    {
        var isProduction = !_env.IsDevelopment();
        var sameSitePolicy = isProduction ? SameSiteMode.None : SameSiteMode.Lax;

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = sameSitePolicy,
            Expires = DateTime.UtcNow.AddHours(1),
            Path = "/"
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = sameSitePolicy,
            Expires = refreshTokenExpiresAt,
            Path = "/"
        };

        Response.Cookies.Append("access_token", accessToken, accessCookieOptions);
        Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);
    }

    private void ClearAuthCookies()
    {
        var isProduction = !_env.IsDevelopment();
        var sameSitePolicy = isProduction ? SameSiteMode.None : SameSiteMode.Lax;

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = sameSitePolicy,
            Path = "/"
        };

        Response.Cookies.Delete("access_token", cookieOptions);
        Response.Cookies.Delete("refresh_token", cookieOptions);
    }
}