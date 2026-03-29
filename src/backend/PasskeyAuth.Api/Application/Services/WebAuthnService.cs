using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace PasskeyAuth.Api.Application.Services;

public class WebAuthnService : IWebAuthnService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WebAuthnService> _logger;

    public WebAuthnService(
        ApplicationDbContext context,
        ILogger<WebAuthnService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public byte[] GenerateChallenge()
    {
        return RandomNumberGenerator.GetBytes(32);
    }

    public async Task<UserInfo> GetUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        return new UserInfo(
            Id: Encoding.UTF8.GetBytes(user.Id.ToString()),
            Name: user.Email,
            DisplayName: user.Name ?? user.Email
        );
    }

    public async Task<byte[][]> GetCredentialsByUserIdAsync(Guid userId)
    {
        var credentials = await _context.PasskeyCredentials
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return credentials.Select(c => Convert.FromBase64String(c.CredentialId)).ToArray();
    }

    public async Task<StoredCredential?> GetCredentialByIdAsync(string credentialId)
    {
        var credential = await _context.PasskeyCredentials
            .FirstOrDefaultAsync(c => c.CredentialId == credentialId);

        if (credential == null)
            return null;

        return new StoredCredential(
            Id: Convert.FromBase64String(credential.CredentialId),
            PublicKey: Convert.FromBase64String(credential.PublicKey),
            UserHandle: Encoding.UTF8.GetBytes(credential.UserId.ToString()),
            Counter: credential.Counter
        );
    }

    public async Task StoreCredentialAsync(
        Guid userId,
        byte[] credentialId,
        byte[] publicKey,
        uint counter,
        string name,
        string deviceType,
        string? userAgent)
    {
        var passkeyCredential = new PasskeyCredential
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CredentialId = Convert.ToBase64String(credentialId),
            PublicKey = Convert.ToBase64String(publicKey),
            Counter = counter,
            Name = name,
            DeviceType = deviceType,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasskeyCredentials.Add(passkeyCredential);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Passkey credential stored. UserId: {UserId}, CredentialId: {CredentialId}", userId, passkeyCredential.CredentialId);
    }

    public async Task UpdateCounterAsync(string credentialId, uint counter)
    {
        var credential = await _context.PasskeyCredentials
            .FirstOrDefaultAsync(c => c.CredentialId == credentialId);

        if (credential == null)
            throw new InvalidOperationException("Credential not found");

        credential.Counter = counter;
        credential.LastUsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
