using Microsoft.EntityFrameworkCore;
using OtpNet;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Text;
using System.Text.Json;

namespace PasskeyAuth.Api.Application.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwoFactorService> _logger;

    public TwoFactorService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<TwoFactorService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GenerateSecretAsync(Guid userId)
    {
        var secret = KeyGeneration.GenerateRandomKey(20); // 160 bits
        var base32Secret = Base32Encoding.ToString(secret);

        var twoFactor = await _context.TwoFactorAuths
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (twoFactor == null)
        {
            twoFactor = new TwoFactorAuth
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SecretKey = base32Secret,
                IsEnabled = false
            };
            _context.TwoFactorAuths.Add(twoFactor);
        }
        else
        {
            twoFactor.SecretKey = base32Secret;
            twoFactor.IsEnabled = false;
        }

        await _context.SaveChangesAsync();
        return base32Secret;
    }

    public async Task<string> GenerateQrCodeUriAsync(Guid userId, string secret, string email)
    {
        var issuer = _configuration["TwoFactor:Issuer"] ?? "Passkey Auth";
        var accountName = email;
        
        // Format: otpauth://totp/{issuer}:{accountName}?secret={secret}&issuer={issuer}
        var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";
        
        return uri;
    }

    public async Task<bool> VerifyCodeAsync(Guid userId, string code)
    {
        var twoFactor = await _context.TwoFactorAuths
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (twoFactor == null || !twoFactor.IsEnabled)
        {
            return false;
        }

        var secretBytes = Base32Encoding.ToBytes(twoFactor.SecretKey);
        var totp = new Totp(secretBytes);
        
        // Verify code with time window tolerance
        var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));

        if (isValid)
        {
            twoFactor.LastVerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return isValid;
    }

    public async Task<List<string>> GenerateBackupCodesAsync(Guid userId)
    {
        var codes = new List<string>();
        var random = new Random();
        
        for (int i = 0; i < 10; i++)
        {
            var code = random.Next(10000000, 99999999).ToString("D8");
            codes.Add(code);
        }

        var twoFactor = await _context.TwoFactorAuths
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (twoFactor != null)
        {
            twoFactor.BackupCodes = JsonSerializer.Serialize(codes);
            await _context.SaveChangesAsync();
        }

        return codes;
    }

    public async Task<bool> VerifyBackupCodeAsync(Guid userId, string code)
    {
        var twoFactor = await _context.TwoFactorAuths
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (twoFactor == null || string.IsNullOrEmpty(twoFactor.BackupCodes))
        {
            return false;
        }

        var backupCodes = JsonSerializer.Deserialize<List<string>>(twoFactor.BackupCodes) ?? new List<string>();
        
        if (backupCodes.Contains(code))
        {
            backupCodes.Remove(code);
            twoFactor.BackupCodes = JsonSerializer.Serialize(backupCodes);
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task EnableTwoFactorAsync(Guid userId, string verificationCode)
    {
        var twoFactor = await _context.TwoFactorAuths
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (twoFactor == null)
        {
            throw new InvalidOperationException("Two-factor authentication not initialized");
        }

        // Verify the code before enabling
        var secretBytes = Base32Encoding.ToBytes(twoFactor.SecretKey);
        var totp = new Totp(secretBytes);
        
        if (!totp.VerifyTotp(verificationCode, out _, new VerificationWindow(1, 1)))
        {
            throw new InvalidOperationException("Invalid verification code");
        }

        twoFactor.IsEnabled = true;
        twoFactor.LastVerifiedAt = DateTime.UtcNow;
        
        // Generate backup codes if not already generated
        if (string.IsNullOrEmpty(twoFactor.BackupCodes))
        {
            var codes = await GenerateBackupCodesAsync(userId);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DisableTwoFactorAsync(Guid userId)
    {
        var twoFactor = await _context.TwoFactorAuths
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (twoFactor != null)
        {
            twoFactor.IsEnabled = false;
            twoFactor.BackupCodes = null;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsTwoFactorEnabledAsync(Guid userId)
    {
        var twoFactor = await _context.TwoFactorAuths
            .FirstOrDefaultAsync(t => t.UserId == userId);

        return twoFactor?.IsEnabled ?? false;
    }
}
