namespace PasskeyAuth.Api.Application.Services;

public interface IWebAuthnService
{
    byte[] GenerateChallenge();
    Task<UserInfo> GetUserAsync(Guid userId);
    Task<byte[][]> GetCredentialsByUserIdAsync(Guid userId);
    Task<StoredCredential?> GetCredentialByIdAsync(string credentialId);
    Task StoreCredentialAsync(Guid userId, byte[] credentialId, byte[] publicKey, uint counter, string name, string deviceType, string? userAgent);
    Task UpdateCounterAsync(string credentialId, uint counter);
}

public record UserInfo(byte[] Id, string Name, string DisplayName);
public record StoredCredential(byte[] Id, byte[] PublicKey, byte[] UserHandle, uint Counter);
