using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BareBonesApi.DTOs.Auth;
using BareBonesApi.Entities.Auth;
using BareBonesApi.Interfaces.Auth;
using Microsoft.Extensions.Configuration;

namespace BareBonesApi.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _config;

    public AuthService(IAuthRepository authRepository, IConfiguration config)
    {
        _authRepository = authRepository;
        _config = config;
    }

    public async Task RegisterAsync(RegisterRequestDto dto)
    {
        var existingEmail = await _authRepository.FindUserByEmailAsync(dto.Email);
        if (existingEmail != null)
            throw new ArgumentException("Email already exists");

        var existingUsername = await _authRepository.FindUserByIdentifierAsync(dto.Username);
        if (existingUsername != null)
            throw new ArgumentException("Username is already taken");

        var user = new User
        {
            Name = dto.Name,
            Username = dto.Username,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Status = "Active" // Defaulting to active for the barebones setup
        };

        await _authRepository.CreateUserAsync(user);
    }

    public async Task<(UserResponseDto User, string AccessToken, string RefreshToken, DateTime ExpiresAt)> LoginAsync(LoginRequestDto dto)
    {
        var user = await _authRepository.FindUserByIdentifierAsync(dto.Identifier);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (user.Status == "Pending")
            throw new UnauthorizedAccessException("Your account is currently pending approval.");

        if (user.Status == "Inactive")
            throw new UnauthorizedAccessException("Your user account is inactive. Please contact support.");

        // Clear any old, expired refresh tokens to keep the DB clean
        await _authRepository.DeleteExpiredTokensAsync(user.Id);

        var (accessToken, refreshToken, expiresAt) = GenerateTokens(user, dto.Remember);

        await _authRepository.SaveRefreshTokenAsync(user.Id, refreshToken, expiresAt);

        var userDto = new UserResponseDto(user.Id, user.Name, user.Username, user.Email, user.Status);

        return (userDto, accessToken, refreshToken, expiresAt);
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> RefreshTokenAsync(string token)
    {
        var storedToken = await _authRepository.FindRefreshTokenAsync(token);

        if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var (newAccessToken, newRefreshToken, expiresAt) = GenerateTokens(storedToken.User, remember: true);

        // Rotate the token: delete the old one and save the new one
        await _authRepository.DeleteRefreshTokenAsync(token);
        await _authRepository.SaveRefreshTokenAsync(storedToken.UserId, newRefreshToken, expiresAt);

        return (newAccessToken, newRefreshToken, expiresAt);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto)
    {
        var user = await _authRepository.FindUserByEmailAsync(dto.Email);
        if (user == null) return; // Prevent email enumeration attacks

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var hashedToken = HashString(rawToken);
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        await _authRepository.SaveResetTokenAsync(user.Email, hashedToken, expiresAt);

        Console.WriteLine($"[SIMULATED EMAIL] Reset Password Link: http://localhost:3000/reset-password?token={rawToken}");
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
        var hashedToken = HashString(dto.Token);
        var user = await _authRepository.FindUserByResetTokenAsync(hashedToken);

        if (user == null)
            throw new ArgumentException("Invalid or expired reset token");

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            throw new ArgumentException("New password must be different from the current password");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _authRepository.UpdateUserAsync(user);
        await _authRepository.ClearResetTokenAsync(user.Id);

        // Invalidate all active sessions across all devices
        await _authRepository.DeleteExpiredTokensAsync(user.Id);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto)
    {
        var user = await _authRepository.FindUserByIdAsync(userId);
        if (user == null) throw new UnauthorizedAccessException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Incorrect current password");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        await _authRepository.UpdateUserAsync(user);

        // Force re-authentication on other devices
        await _authRepository.DeleteExpiredTokensAsync(userId);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authRepository.DeleteRefreshTokenAsync(refreshToken);
        }
    }

    // Private Helper Methods

    private (string AccessToken, string RefreshToken, DateTime ExpiresAt) GenerateTokens(User user, bool remember)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name)
        };

        var key = new SymmetricSecurityKey(secretKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(jwtToken);

        // Generate Refresh Token
        var refreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(40));
        var expiresInHours = remember ? 30 * 24 : 24;
        var expiresAt = DateTime.UtcNow.AddHours(expiresInHours);

        return (accessToken, refreshToken, expiresAt);
    }

    private string HashString(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLower();
    }
}