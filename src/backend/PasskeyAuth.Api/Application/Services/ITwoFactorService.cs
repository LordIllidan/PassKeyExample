namespace PasskeyAuth.Api.Application.Services;

public interface ITwoFactorService
{
    Task<string> GenerateSecretAsync(Guid userId);
    Task<string> GenerateQrCodeUriAsync(Guid userId, string secret, string email);
    Task<bool> VerifyCodeAsync(Guid userId, string code);
    Task<List<string>> GenerateBackupCodesAsync(Guid userId);
    Task<bool> VerifyBackupCodeAsync(Guid userId, string code);
    Task EnableTwoFactorAsync(Guid userId, string verificationCode);
    Task DisableTwoFactorAsync(Guid userId);
    Task<bool> IsTwoFactorEnabledAsync(Guid userId);
}
