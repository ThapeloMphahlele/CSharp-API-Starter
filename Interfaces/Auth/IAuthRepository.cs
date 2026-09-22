using BareBonesApi.Entities.Auth;

namespace BareBonesApi.Interfaces.Auth;

public interface IAuthRepository
{
    Task<User?> FindUserByEmailAsync(string email);
    Task<User?> FindUserByIdentifierAsync(string identifier);
    Task<User?> FindUserByIdAsync(Guid userId);
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task SaveResetTokenAsync(string email, string hashedToken, DateTime expiresAt);
    Task<User?> FindUserByResetTokenAsync(string hashedToken);
    Task ClearResetTokenAsync(Guid userId);
    Task SaveRefreshTokenAsync(Guid userId, string token, DateTime expiresAt);
    Task<RefreshToken?> FindRefreshTokenAsync(string token);
    Task DeleteRefreshTokenAsync(string token);
    Task DeleteExpiredTokensAsync(Guid userId);
}