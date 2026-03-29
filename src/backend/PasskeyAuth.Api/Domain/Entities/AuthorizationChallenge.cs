using PasskeyAuth.Api.Domain.Entities;

namespace PasskeyAuth.Api.Domain.Entities;

public enum AuthorizationChallengeStatus
{
    Pending = 1,
    Verified = 2,
    Expired = 3,
    Failed = 4
}

public class AuthorizationChallenge
{
    public Guid ChallengeId { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string OperationType { get; set; } = string.Empty; // max 100
    public string OperationData { get; set; } = string.Empty; // JSONB
    public string? ChallengeCode { get; set; } // max 10
    public AuthorizationChallengeStatus Status { get; set; } = AuthorizationChallengeStatus.Pending;
    public TwoFactorMethodType? MethodType { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    public int FailedAttempts { get; set; } = 0;
    
    // Navigation property
    public AuthorizationToken? AuthorizationToken { get; set; }
}

