using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Net;
using Xunit;

namespace PasskeyAuth.Api.Tests.Domain.Entities;

public class AuditLogTests
{
    [Fact]
    public void AuditLog_ShouldBeCreated_WithAllRequiredProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var eventType = "2fa.code.verify";
        var eventCategory = "2FA";
        var details = "{\"methodType\":\"sms\",\"success\":true}";
        var ipAddress = IPAddress.Parse("192.168.1.1");
        
        // Act
        var log = new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            EventCategory = eventCategory,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = "Mozilla/5.0",
            Success = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, log.LogId);
        Assert.Equal(userId, log.UserId);
        Assert.Equal(eventType, log.EventType);
        Assert.Equal(eventCategory, log.EventCategory);
        Assert.Equal(details, log.Details);
        Assert.Equal(ipAddress, log.IpAddress);
        Assert.True(log.Success);
    }

    [Fact]
    public async Task AuditLog_ShouldBeSaved_ToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var log = new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            EventType = "2fa.code.verify",
            EventCategory = "2FA",
            Details = "{\"methodType\":\"sms\"}",
            IpAddress = IPAddress.Parse("192.168.1.1"),
            Success = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.AuditLogs.Add(log);
        await context.SaveChangesAsync();
        
        // Assert
        var saved = await context.AuditLogs
            .FirstOrDefaultAsync(l => l.LogId == log.LogId);
        
        Assert.NotNull(saved);
        Assert.Equal(log.EventType, saved.EventType);
        Assert.Equal(log.Success, saved.Success);
    }

    [Fact]
    public void AuditLog_ShouldHandle_NullableUserId()
    {
        // Arrange & Act
        var log = new AuditLog
        {
            LogId = Guid.NewGuid(),
            UserId = null, // System event without user
            EventType = "system.startup",
            EventCategory = "System",
            Details = "{}",
            Success = true,
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.Null(log.UserId);
        Assert.NotNull(log);
    }

    [Fact]
    public void AuditLog_ShouldHandle_ErrorMessages()
    {
        // Arrange & Act
        var log = new AuditLog
        {
            LogId = Guid.NewGuid(),
            EventType = "2fa.code.verify",
            EventCategory = "2FA",
            Details = "{}",
            Success = false,
            ErrorMessage = "Invalid code",
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.False(log.Success);
        Assert.Equal("Invalid code", log.ErrorMessage);
    }
}


