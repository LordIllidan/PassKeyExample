namespace PasskeyAuth.Api.Domain.Entities;

public class AuthorizationToken
{
    public Guid TokenId { get; set; }
    public Guid ChallengeId { get; set; }
    public AuthorizationChallenge Challenge { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string OperationType { get; set; } = string.Empty; // max 100
    public string Token { get; set; } = string.Empty; // JWT
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

