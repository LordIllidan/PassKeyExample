namespace PasskeyAuth.Api.Domain.Entities;

public class PasskeyCredential
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    // WebAuthn fields
    public string CredentialId { get; set; } = string.Empty; // Base64 encoded
    public string PublicKey { get; set; } = string.Empty; // JSON encoded
    public uint Counter { get; set; }
    
    // Metadata
    public string Name { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // "platform", "cross-platform"
    public string? UserAgent { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
}


