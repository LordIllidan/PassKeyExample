using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using PasskeyAuth.Api.Application.Services;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Text.Json;
using Xunit;

namespace PasskeyAuth.Api.Tests.Application.Services;

public class TwoFactorMethodServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<TwoFactorMethodService>> _loggerMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IPushNotificationService> _pushServiceMock;
    private readonly TwoFactorMethodService _service;

    public TwoFactorMethodServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        
        _loggerMock = new Mock<ILogger<TwoFactorMethodService>>();
        _smsServiceMock = new Mock<ISmsService>();
        _emailServiceMock = new Mock<IEmailService>();
        _pushServiceMock = new Mock<IPushNotificationService>();

        _service = new TwoFactorMethodService(
            _context,
            _cache,
            _loggerMock.Object,
            _smsServiceMock.Object,
            _emailServiceMock.Object,
            _pushServiceMock.Object);
    }

    [Fact]
    public async Task GenerateCodeAsync_ShouldGenerateCode_ForTOTP()
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
        var code = await _service.GenerateCodeAsync(userId, TwoFactorMethodType.TOTP);

        // Assert
        Assert.NotNull(code);
        Assert.Matches(@"^\d{6}$", code); // 6 digits
        Assert.True(_cache.TryGetValue($"2fa:totp:{userId}:{code}", out _));
    }

    [Fact]
    public async Task GenerateCodeAsync_ShouldGenerateAndSendCode_ForSMS()
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

        var method = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = TwoFactorMethodType.SMS,
            IsEnabled = true,
            Configuration = JsonSerializer.Serialize(new Dictionary<string, string> { { "phoneNumber", "+48123456789" } })
        };
        _context.TwoFactorMethods.Add(method);
        await _context.SaveChangesAsync();

        _smsServiceMock.Setup(s => s.SendCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string phone, string code) => code);

        // Act
        var code = await _service.GenerateCodeAsync(userId, TwoFactorMethodType.SMS);

        // Assert
        Assert.NotNull(code);
        Assert.Matches(@"^\d{6}$", code); // 6 digits
        _smsServiceMock.Verify(s => s.SendCodeAsync("+48123456789", code), Times.Once);
        Assert.True(_cache.TryGetValue($"2fa:sms:{userId}:{code}", out _));
    }

    [Fact]
    public async Task GenerateCodeAsync_ShouldGenerateAndSendCode_ForEmail()
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

        _emailServiceMock.Setup(s => s.SendCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string email, string code) => code);

        // Act
        var code = await _service.GenerateCodeAsync(userId, TwoFactorMethodType.Email);

        // Assert
        Assert.NotNull(code);
        Assert.Matches(@"^\d{6}$", code); // 6 digits
        _emailServiceMock.Verify(s => s.SendCodeAsync("test@example.com", code), Times.Once);
        Assert.True(_cache.TryGetValue($"2fa:email:{userId}:{code}", out _));
    }

    [Fact]
    public async Task GenerateCodeAsync_ShouldGenerateApprovalCode_ForPush()
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

        var method = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = TwoFactorMethodType.Push,
            IsEnabled = true,
            Configuration = JsonSerializer.Serialize(new Dictionary<string, string> { { "deviceId", "device-123" } })
        };
        _context.TwoFactorMethods.Add(method);
        await _context.SaveChangesAsync();

        _pushServiceMock.Setup(s => s.SendApprovalRequestAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string deviceId, string userId) => $"PUSH-{Random.Shared.Next(1000, 9999)}");

        // Act
        var code = await _service.GenerateCodeAsync(userId, TwoFactorMethodType.Push);

        // Assert
        Assert.NotNull(code);
        Assert.StartsWith("PUSH-", code);
        _pushServiceMock.Verify(s => s.SendApprovalRequestAsync("device-123", userId.ToString()), Times.Once);
        Assert.True(_cache.TryGetValue($"2fa:push:{userId}:{code}", out _));
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnTrue_ForValidCachedCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var cacheKey = $"2fa:sms:{userId}:{code}";
        _cache.Set(cacheKey, true, TimeSpan.FromMinutes(10));

        // Act
        var result = await _service.VerifyCodeAsync(userId, TwoFactorMethodType.SMS, code);

        // Assert
        Assert.True(result);
        Assert.False(_cache.TryGetValue(cacheKey, out _)); // Should be removed after verification
    }

    [Fact]
    public async Task VerifyCodeAsync_ShouldReturnFalse_ForInvalidCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "999999";

        // Act
        var result = await _service.VerifyCodeAsync(userId, TwoFactorMethodType.SMS, code);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetupMethodAsync_ShouldCreateMethod_WhenUserHasNoPrimary()
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

        var configuration = new Dictionary<string, string> { { "phoneNumber", "+48123456789" } };

        // Act
        var result = await _service.SetupMethodAsync(userId, TwoFactorMethodType.SMS, configuration);

        // Assert
        Assert.NotNull(result);
        var method = await _context.TwoFactorMethods
            .FirstOrDefaultAsync(m => m.UserId == userId && m.MethodType == TwoFactorMethodType.SMS);
        Assert.NotNull(method);
        Assert.True(method.IsEnabled);
        Assert.True(method.IsPrimary); // Should be primary as it's the first method
    }

    [Fact]
    public async Task SetupMethodAsync_ShouldNotSetPrimary_WhenUserHasPrimary()
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

        var existingMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = TwoFactorMethodType.SMS,
            IsEnabled = true,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.TwoFactorMethods.Add(existingMethod);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.SetupMethodAsync(userId, TwoFactorMethodType.Email, null);

        // Assert
        var newMethod = await _context.TwoFactorMethods
            .FirstOrDefaultAsync(m => m.UserId == userId && m.MethodType == TwoFactorMethodType.Email);
        Assert.NotNull(newMethod);
        Assert.False(newMethod.IsPrimary); // Should not be primary
    }

    [Fact]
    public async Task GetUserMethodsAsync_ShouldReturnMethods_OrderedByPrimary()
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

        var method1 = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = TwoFactorMethodType.SMS,
            IsEnabled = true,
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        };

        var method2 = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = TwoFactorMethodType.Email,
            IsEnabled = true,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.TwoFactorMethods.AddRange(method1, method2);
        await _context.SaveChangesAsync();

        // Act
        var methods = await _service.GetUserMethodsAsync(userId);

        // Assert
        Assert.Equal(2, methods.Count);
        Assert.True(methods[0].IsPrimary); // Primary should be first
        Assert.Equal(TwoFactorMethodType.Email, methods[0].MethodType);
    }

    [Fact]
    public async Task SetPrimaryMethodAsync_ShouldSetOnlyOnePrimary()
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

        var method1 = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = TwoFactorMethodType.SMS,
            IsEnabled = true,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
        };

        var method2 = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = TwoFactorMethodType.Email,
            IsEnabled = true,
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.TwoFactorMethods.AddRange(method1, method2);
        await _context.SaveChangesAsync();

        // Act
        await _service.SetPrimaryMethodAsync(userId, method2.Id);

        // Assert
        var updated = await _context.TwoFactorMethods.ToListAsync();
        Assert.Single(updated, m => m.IsPrimary && m.Id == method2.Id);
        Assert.All(updated.Where(m => m.Id != method2.Id), m => Assert.False(m.IsPrimary));
    }

    [Fact]
    public async Task DisableMethodAsync_ShouldDisableMethod()
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

        var method = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = TwoFactorMethodType.SMS,
            IsEnabled = true,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.TwoFactorMethods.Add(method);
        await _context.SaveChangesAsync();

        // Act
        await _service.DisableMethodAsync(userId, method.Id);

        // Assert
        var updated = await _context.TwoFactorMethods.FindAsync(method.Id);
        Assert.NotNull(updated);
        Assert.False(updated.IsEnabled);
    }
}
