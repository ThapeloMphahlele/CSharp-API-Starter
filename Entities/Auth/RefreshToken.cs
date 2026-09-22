namespace BareBonesApi.Entities.Auth;

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    // Navigation Property for the Many-To-One relationship
    public User User { get; set; } = null!;
}