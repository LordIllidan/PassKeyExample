namespace PasskeyAuth.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<PasskeyCredential> PasskeyCredentials { get; set; } = new List<PasskeyCredential>();
    public TwoFactorAuth? TwoFactorAuth { get; set; }
    public ICollection<TwoFactorMethod> TwoFactorMethods { get; set; } = new List<TwoFactorMethod>();
}
