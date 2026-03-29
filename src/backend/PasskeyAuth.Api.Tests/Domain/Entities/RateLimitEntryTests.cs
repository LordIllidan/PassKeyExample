using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Net;
using Xunit;

namespace PasskeyAuth.Api.Tests.Domain.Entities;

public class RateLimitEntryTests
{
    [Fact]
    public void RateLimitEntry_ShouldBeCreated_WithAllRequiredProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ipAddress = IPAddress.Parse("192.168.1.1");
        var operationType = "2fa.code.generate";
        var windowStart = DateTime.UtcNow;
        var windowEnd = windowStart.AddMinutes(60);
        
        // Act
        var entry = new RateLimitEntry
        {
            EntryId = Guid.NewGuid(),
            UserId = userId,
            IpAddress = ipAddress,
            OperationType = operationType,
            AttemptCount = 1,
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, entry.EntryId);
        Assert.Equal(userId, entry.UserId);
        Assert.Equal(ipAddress, entry.IpAddress);
        Assert.Equal(operationType, entry.OperationType);
        Assert.Equal(1, entry.AttemptCount);
        Assert.False(entry.IsBlocked);
    }

    [Fact]
    public async Task RateLimitEntry_ShouldBeSaved_ToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var entry = new RateLimitEntry
        {
            EntryId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            IpAddress = IPAddress.Parse("192.168.1.1"),
            OperationType = "2fa.code.generate",
            AttemptCount = 1,
            WindowStart = DateTime.UtcNow,
            WindowEnd = DateTime.UtcNow.AddMinutes(60),
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.RateLimitEntries.Add(entry);
        await context.SaveChangesAsync();
        
        // Assert
        var saved = await context.RateLimitEntries
            .FirstOrDefaultAsync(e => e.EntryId == entry.EntryId);
        
        Assert.NotNull(saved);
        Assert.Equal(entry.OperationType, saved.OperationType);
        Assert.Equal(entry.AttemptCount, saved.AttemptCount);
    }

    [Fact]
    public async Task RateLimitEntry_ShouldIncrementAttemptCount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var entry = new RateLimitEntry
        {
            EntryId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            OperationType = "2fa.code.generate",
            AttemptCount = 1,
            WindowStart = DateTime.UtcNow,
            WindowEnd = DateTime.UtcNow.AddMinutes(60),
            CreatedAt = DateTime.UtcNow
        };
        
        context.RateLimitEntries.Add(entry);
        await context.SaveChangesAsync();
        
        // Act
        entry.AttemptCount++;
        await context.SaveChangesAsync();
        
        // Assert
        var updated = await context.RateLimitEntries
            .FirstOrDefaultAsync(e => e.EntryId == entry.EntryId);
        
        Assert.NotNull(updated);
        Assert.Equal(2, updated.AttemptCount);
    }

    [Fact]
    public async Task RateLimitEntry_ShouldBlock_WhenThresholdExceeded()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var entry = new RateLimitEntry
        {
            EntryId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            OperationType = "2fa.code.generate",
            AttemptCount = 5, // Threshold exceeded
            WindowStart = DateTime.UtcNow,
            WindowEnd = DateTime.UtcNow.AddMinutes(60),
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow
        };
        
        context.RateLimitEntries.Add(entry);
        await context.SaveChangesAsync();
        
        // Act
        entry.IsBlocked = true;
        entry.BlockedUntil = DateTime.UtcNow.AddMinutes(30);
        await context.SaveChangesAsync();
        
        // Assert
        var updated = await context.RateLimitEntries
            .FirstOrDefaultAsync(e => e.EntryId == entry.EntryId);
        
        Assert.NotNull(updated);
        Assert.True(updated.IsBlocked);
        Assert.NotNull(updated.BlockedUntil);
    }

    [Fact]
    public void RateLimitEntry_ShouldHandle_NullableUserId()
    {
        // Arrange & Act
        var entry = new RateLimitEntry
        {
            EntryId = Guid.NewGuid(),
            UserId = null, // IP-based rate limiting
            IpAddress = IPAddress.Parse("192.168.1.1"),
            OperationType = "2fa.code.generate",
            AttemptCount = 1,
            WindowStart = DateTime.UtcNow,
            WindowEnd = DateTime.UtcNow.AddMinutes(60),
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.Null(entry.UserId);
        Assert.NotNull(entry.IpAddress);
    }

    [Fact]
    public void RateLimitEntry_ShouldCalculateWindow_Correctly()
    {
        // Arrange
        var windowStart = DateTime.UtcNow;
        var windowEnd = windowStart.AddMinutes(60);
        
        // Act
        var entry = new RateLimitEntry
        {
            EntryId = Guid.NewGuid(),
            OperationType = "2fa.code.generate",
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        var windowDuration = entry.WindowEnd - entry.WindowStart;
        Assert.Equal(TimeSpan.FromMinutes(60), windowDuration);
    }
}


