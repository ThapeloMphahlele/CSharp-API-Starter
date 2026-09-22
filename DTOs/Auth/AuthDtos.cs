namespace BareBonesApi.DTOs.Auth;

public record RegisterRequestDto(
    string Name,
    string Username,
    string Email,
    string? Phone,
    string Password,
    bool AgreedToTerms);

public record LoginRequestDto(
    string Identifier,
    string Password,
    bool Remember = false);

public record ForgotPasswordRequestDto(string Email);

public record ResetPasswordRequestDto(
    string Token,
    string NewPassword,
    string ConfirmPassword);

public record ChangePasswordRequestDto(
    string CurrentPassword,
    string NewPassword);

// Sent back to the client upon successful login/registration
public record UserResponseDto(
    Guid Id,
    string Name,
    string Username,
    string Email,
    string Status);