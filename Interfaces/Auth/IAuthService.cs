using BareBonesApi.DTOs.Auth;

namespace BareBonesApi.Interfaces.Auth;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequestDto dto);
    Task<(UserResponseDto User, string AccessToken, string RefreshToken, DateTime ExpiresAt)> LoginAsync(LoginRequestDto dto);
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> RefreshTokenAsync(string token);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto dto);
    Task ResetPasswordAsync(ResetPasswordRequestDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto);
    Task LogoutAsync(string refreshToken);
}