namespace PasskeyAuth.Api.Domain.Entities;

public class TwoFactorAuth
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public bool IsEnabled { get; set; }
    public string SecretKey { get; set; } = string.Empty; // Base32 encoded secret
    public string? BackupCodes { get; set; } // JSON array of backup codes
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastVerifiedAt { get; set; }
}

