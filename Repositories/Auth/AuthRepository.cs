using Microsoft.EntityFrameworkCore;
using BareBonesApi.Entities.Auth;
using BareBonesApi.Interfaces.Auth;
using BareBonesApi.Entities;

namespace BareBonesApi.Repositories.Auth;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;

    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> FindUserByIdentifierAsync(string identifier)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);
    }

    public async Task<User?> FindUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task SaveResetTokenAsync(string email, string hashedToken, DateTime expiresAt)
    {
        var user = await FindUserByEmailAsync(email);
        if (user != null)
        {
            user.ResetToken = hashedToken;
            user.ResetTokenExpiresAt = expiresAt;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User?> FindUserByResetTokenAsync(string hashedToken)
    {
        return await _context.Users.FirstOrDefaultAsync(u =>
            u.ResetToken == hashedToken &&
            u.ResetTokenExpiresAt > DateTime.UtcNow);
    }

    public async Task ClearResetTokenAsync(Guid userId)
    {
        var user = await FindUserByIdAsync(userId);
        if (user != null)
        {
            user.ResetToken = null;
            user.ResetTokenExpiresAt = null;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string token, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> FindRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task DeleteRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens.FindAsync(token);
        if (refreshToken != null)
        {
            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiredTokensAsync(Guid userId)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}