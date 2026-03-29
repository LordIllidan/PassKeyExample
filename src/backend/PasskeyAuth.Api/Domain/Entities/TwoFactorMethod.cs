namespace PasskeyAuth.Api.Domain.Entities;

public enum TwoFactorMethodType
{
    TOTP = 1,
    U2F = 2,
    SMS = 3,
    Email = 4,
    Push = 5
}

public class TwoFactorMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public TwoFactorMethodType MethodType { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsPrimary { get; set; }
    
    // Method-specific data (JSON)
    public string? Configuration { get; set; } // JSON with method-specific settings
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
}

