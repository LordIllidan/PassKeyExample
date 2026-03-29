using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OtpNet;
using PasskeyAuth.Api.Application.Services;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Text.Json;
using Xunit;

namespace PasskeyAuth.Api.Tests.Application.Services;

public class TwoFactorServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<TwoFactorService>> _loggerMock;
    private readonly TwoFactorService _service;

    public TwoFactorServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        
        var configDict = new Dictionary<string, string?>
        {
            { "TwoFactor:Issuer", "Test App" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        _loggerMock = new Mock<ILogger<TwoFactorService>>();
        _service = new TwoFactorService(_context, _configuration, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerateSecretAsync_ShouldCreateNewSecret_WhenTwoFactorAuthDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var secret = await _service.GenerateSecretAsync(userId);

        // Assert
        Assert.NotNull(secret);
        Assert.NotEmpty(secret);
        var twoFactor = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(twoFactor);
        Assert.Equal(secret, twoFactor.SecretKey);
        Assert.False(twoFactor.IsEnabled);
    }

    [Fact]
    public async Task GenerateSecretAsync_ShouldUpdateExistingSecret_WhenTwoFactorAuthExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        
        var existingSecret = "OLD_SECRET_KEY";
        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = existingSecret,
            IsEnabled = true
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        var newSecret = await _service.GenerateSecretAsync(userId);

        // Assert
        Assert.NotNull(newSecret);
        Assert.NotEqual(existingSecret, newSecret);
        var updated = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(updated);
        Assert.Equal(newSecret, updated.SecretKey);
        Assert.False(updated.IsEnabled); // Should be disabled when regenerating
    }

    [Fact]
    public async Task GenerateQrCodeUriAsync_ShouldReturnValidUri()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var secret = "JBSWY3DPEHPK3PXP";
        var email = "test@example.com";

        // Act
        var uri = await _service.GenerateQrCodeUriAsync(userId, secret, email);

        // Assert
        Assert.NotNull(uri);
        Assert.StartsWith("otpauth://totp/", uri);
        Assert.Contains("secret=" + secret, uri);
        Assert.Contains("issuer=", uri);
        Assert.Contains("algorithm=SHA1", uri);
        Assert.Contains("digits=6", uri);
        Assert.Contains("period=30", uri);
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnTrue_ForValidCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var secret = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secret);
        var totp = new Totp(secret);
        var validCode = totp.ComputeTotp();

        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = base32Secret,
            IsEnabled = true
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyCodeAsync(userId, validCode);

        // Assert
        Assert.True(result);
        var updated = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(updated);
        Assert.NotNull(updated.LastVerifiedAt);
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnFalse_ForInvalidCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var secret = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secret);

        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = base32Secret,
            IsEnabled = true
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyCodeAsync(userId, "000000");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnFalse_WhenTwoFactorNotEnabled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var secret = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secret);

        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = base32Secret,
            IsEnabled = false // Not enabled
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        var totp = new Totp(secret);
        var code = totp.ComputeTotp();

        // Act
        var result = await _service.VerifyCodeAsync(userId, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GenerateBackupCodesAsync_ShouldGenerate10Codes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = "JBSWY3DPEHPK3PXP",
            IsEnabled = true
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        var codes = await _service.GenerateBackupCodesAsync(userId);

        // Assert
        Assert.NotNull(codes);
        Assert.Equal(10, codes.Count);
        Assert.All(codes, code => Assert.Matches(@"^\d{8}$", code)); // 8 digits

        var updated = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(updated);
        Assert.NotNull(updated.BackupCodes);
        var savedCodes = JsonSerializer.Deserialize<List<string>>(updated.BackupCodes);
        Assert.NotNull(savedCodes);
        Assert.Equal(10, savedCodes.Count);
    }

    [Fact]
    public async Task VerifyBackupCodeAsync_ShouldReturnTrue_ForValidCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var backupCodes = new List<string> { "12345678", "87654321", "11223344" };
        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = "JBSWY3DPEHPK3PXP",
            IsEnabled = true,
            BackupCodes = JsonSerializer.Serialize(backupCodes)
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyBackupCodeAsync(userId, "12345678");

        // Assert
        Assert.True(result);
        var updated = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(updated);
        var remainingCodes = JsonSerializer.Deserialize<List<string>>(updated.BackupCodes ?? "[]");
        Assert.NotNull(remainingCodes);
        Assert.DoesNotContain("12345678", remainingCodes);
        Assert.Equal(2, remainingCodes.Count);
    }

    [Fact]
    public async Task VerifyBackupCodeAsync_ShouldReturnFalse_ForInvalidCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var backupCodes = new List<string> { "12345678", "87654321" };
        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = "JBSWY3DPEHPK3PXP",
            IsEnabled = true,
            BackupCodes = JsonSerializer.Serialize(backupCodes)
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.VerifyBackupCodeAsync(userId, "99999999");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EnableTwoFactorAsync_ShouldEnable2FA_WhenCodeIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var secret = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secret);
        var totp = new Totp(secret);
        var verificationCode = totp.ComputeTotp();

        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = base32Secret,
            IsEnabled = false
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        await _service.EnableTwoFactorAsync(userId, verificationCode);

        // Assert
        var updated = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(updated);
        Assert.True(updated.IsEnabled);
        Assert.NotNull(updated.LastVerifiedAt);
        Assert.NotNull(updated.BackupCodes); // Should generate backup codes
    }

    [Fact]
    public async Task EnableTwoFactorAsync_ShouldThrowException_WhenCodeIsInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var secret = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secret);

        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = base32Secret,
            IsEnabled = false
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EnableTwoFactorAsync(userId, "000000"));
    }

    [Fact]
    public async Task DisableTwoFactorAsync_ShouldDisable2FA()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = "JBSWY3DPEHPK3PXP",
            IsEnabled = true,
            BackupCodes = "[\"12345678\"]"
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        await _service.DisableTwoFactorAsync(userId);

        // Assert
        var updated = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
        Assert.NotNull(updated);
        Assert.False(updated.IsEnabled);
        Assert.Null(updated.BackupCodes);
    }

    [Fact]
    public async Task IsTwoFactorEnabledAsync_ShouldReturnTrue_WhenEnabled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);

        var twoFactor = new TwoFactorAuth
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SecretKey = "JBSWY3DPEHPK3PXP",
            IsEnabled = true
        };
        _context.TwoFactorAuths.Add(twoFactor);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsTwoFactorEnabledAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsTwoFactorEnabledAsync_ShouldReturnFalse_WhenNotEnabled()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.IsTwoFactorEnabledAsync(userId);

        // Assert
        Assert.False(result);
    }
}
