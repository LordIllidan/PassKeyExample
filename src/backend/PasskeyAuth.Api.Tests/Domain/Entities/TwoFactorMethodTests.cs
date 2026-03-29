using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using Xunit;

namespace PasskeyAuth.Api.Tests.Domain.Entities;

public class TwoFactorMethodTests
{
    [Fact]
    public void TwoFactorMethod_ShouldBeCreated_WithAllRequiredProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var methodType = TwoFactorMethodType.SMS;
        
        // Act
        var method = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MethodType = methodType,
            IsEnabled = true,
            IsPrimary = false,
            Configuration = null,
            CreatedAt = DateTime.UtcNow,
            LastUsedAt = null
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, method.Id);
        Assert.Equal(userId, method.UserId);
        Assert.Equal(methodType, method.MethodType);
        Assert.True(method.IsEnabled);
        Assert.False(method.IsPrimary);
        Assert.Null(method.Configuration);
        Assert.NotEqual(default(DateTime), method.CreatedAt);
        Assert.Null(method.LastUsedAt);
    }

    [Fact]
    public void TwoFactorMethod_ShouldHaveRelationship_WithUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        var method = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            MethodType = TwoFactorMethodType.Email,
            User = user
        };
        
        // Act & Assert
        Assert.NotNull(method.User);
        Assert.Equal(user.Id, method.User.Id);
        Assert.Equal(user.Email, method.User.Email);
    }

    [Fact]
    public async Task TwoFactorMethod_ShouldBeSaved_ToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        var method = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            MethodType = TwoFactorMethodType.SMS,
            IsEnabled = true,
            IsPrimary = true,
            Configuration = "{\"phoneNumber\":\"+48123456789\"}",
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.Users.Add(user);
        context.TwoFactorMethods.Add(method);
        await context.SaveChangesAsync();
        
        // Assert
        var saved = await context.TwoFactorMethods
            .FirstOrDefaultAsync(m => m.Id == method.Id);
        
        Assert.NotNull(saved);
        Assert.Equal(method.UserId, saved.UserId);
        Assert.Equal(method.MethodType, saved.MethodType);
        Assert.Equal(method.Configuration, saved.Configuration);
    }

    [Fact]
    public async Task TwoFactorMethod_ShouldBeDeleted_WhenUserIsDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        var method = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            MethodType = TwoFactorMethodType.Push,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        context.TwoFactorMethods.Add(method);
        await context.SaveChangesAsync();
        
        // Act
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        
        // Assert
        var deleted = await context.TwoFactorMethods
            .FirstOrDefaultAsync(m => m.Id == method.Id);
        
        Assert.Null(deleted);
    }

    [Fact]
    public void TwoFactorMethod_ShouldValidate_EnumValues()
    {
        // Arrange & Act
        var totpMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MethodType = TwoFactorMethodType.TOTP
        };
        
        var smsMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MethodType = TwoFactorMethodType.SMS
        };
        
        var emailMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MethodType = TwoFactorMethodType.Email
        };
        
        var pushMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MethodType = TwoFactorMethodType.Push
        };
        
        var u2fMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MethodType = TwoFactorMethodType.U2F
        };
        
        // Assert
        Assert.Equal(TwoFactorMethodType.TOTP, totpMethod.MethodType);
        Assert.Equal(TwoFactorMethodType.SMS, smsMethod.MethodType);
        Assert.Equal(TwoFactorMethodType.Email, emailMethod.MethodType);
        Assert.Equal(TwoFactorMethodType.Push, pushMethod.MethodType);
        Assert.Equal(TwoFactorMethodType.U2F, u2fMethod.MethodType);
    }

    [Fact]
    public void TwoFactorMethod_ShouldHandle_ConfigurationAsJson()
    {
        // Arrange
        var smsConfig = "{\"phoneNumber\":\"+48123456789\"}";
        var emailConfig = "{\"email\":\"user@example.com\"}";
        var pushConfig = "{\"deviceId\":\"device-uuid\",\"deviceName\":\"iPhone 13\"}";
        
        // Act
        var smsMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MethodType = TwoFactorMethodType.SMS,
            Configuration = smsConfig
        };
        
        var emailMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MethodType = TwoFactorMethodType.Email,
            Configuration = emailConfig
        };
        
        var pushMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            MethodType = TwoFactorMethodType.Push,
            Configuration = pushConfig
        };
        
        // Assert
        Assert.NotNull(smsMethod.Configuration);
        Assert.Equal(smsConfig, smsMethod.Configuration);
        Assert.NotNull(emailMethod.Configuration);
        Assert.Equal(emailConfig, emailMethod.Configuration);
        Assert.NotNull(pushMethod.Configuration);
        Assert.Equal(pushConfig, pushMethod.Configuration);
    }

    [Fact]
    public async Task TwoFactorMethod_ShouldSupport_MultipleMethodsPerUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        var smsMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            MethodType = TwoFactorMethodType.SMS,
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow
        };
        
        var emailMethod = new TwoFactorMethod
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            MethodType = TwoFactorMethodType.Email,
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.Users.Add(user);
        context.TwoFactorMethods.Add(smsMethod);
        context.TwoFactorMethods.Add(emailMethod);
        await context.SaveChangesAsync();
        
        // Assert
        var methods = await context.TwoFactorMethods
            .Where(m => m.UserId == user.Id)
            .ToListAsync();
        
        Assert.Equal(2, methods.Count);
        Assert.Single(methods, m => m.IsPrimary);
        Assert.Single(methods, m => m.MethodType == TwoFactorMethodType.SMS);
        Assert.Single(methods, m => m.MethodType == TwoFactorMethodType.Email);
    }
}
