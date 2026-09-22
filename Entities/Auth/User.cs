namespace BareBonesApi.Entities.Auth;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    // Password Reset Fields
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiresAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property for the One-To-Many relationship
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}