using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using Xunit;

namespace PasskeyAuth.Api.Tests.Domain.Entities;

public class OutboxMessageTests
{
    [Fact]
    public void OutboxMessage_ShouldBeCreated_WithAllRequiredProperties()
    {
        // Arrange
        var eventType = "2fa.code.generate.sms";
        var payload = "{\"userId\":\"guid\",\"code\":\"123456\"}";
        
        // Act
        var message = new OutboxMessage
        {
            MessageId = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            Status = OutboxMessageStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, message.MessageId);
        Assert.Equal(eventType, message.EventType);
        Assert.Equal(payload, message.Payload);
        Assert.Equal(OutboxMessageStatus.Pending, message.Status);
        Assert.Equal(0, message.RetryCount);
        Assert.Null(message.PublishedAt);
        Assert.Null(message.ProcessedAt);
    }

    [Fact]
    public async Task OutboxMessage_ShouldBeSaved_ToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var message = new OutboxMessage
        {
            MessageId = Guid.NewGuid(),
            EventType = "2fa.code.generate.sms",
            Payload = "{\"userId\":\"guid\"}",
            Status = OutboxMessageStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
        
        // Assert
        var saved = await context.OutboxMessages
            .FirstOrDefaultAsync(m => m.MessageId == message.MessageId);
        
        Assert.NotNull(saved);
        Assert.Equal(message.EventType, saved.EventType);
        Assert.Equal(message.Status, saved.Status);
    }

    [Fact]
    public async Task OutboxMessage_ShouldTransition_FromPendingToPublished()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var message = new OutboxMessage
        {
            MessageId = Guid.NewGuid(),
            EventType = "2fa.code.generate.sms",
            Payload = "{}",
            Status = OutboxMessageStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
        
        // Act
        message.Status = OutboxMessageStatus.Published;
        message.PublishedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        
        // Assert
        var updated = await context.OutboxMessages
            .FirstOrDefaultAsync(m => m.MessageId == message.MessageId);
        
        Assert.NotNull(updated);
        Assert.Equal(OutboxMessageStatus.Published, updated.Status);
        Assert.NotNull(updated.PublishedAt);
    }

    [Fact]
    public async Task OutboxMessage_ShouldTransition_FromPublishedToProcessed()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var message = new OutboxMessage
        {
            MessageId = Guid.NewGuid(),
            EventType = "2fa.code.generate.sms",
            Payload = "{}",
            Status = OutboxMessageStatus.Published,
            PublishedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
        
        // Act
        message.Status = OutboxMessageStatus.Processed;
        message.ProcessedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        
        // Assert
        var updated = await context.OutboxMessages
            .FirstOrDefaultAsync(m => m.MessageId == message.MessageId);
        
        Assert.NotNull(updated);
        Assert.Equal(OutboxMessageStatus.Processed, updated.Status);
        Assert.NotNull(updated.ProcessedAt);
    }

    [Fact]
    public async Task OutboxMessage_ShouldIncrementRetryCount_OnFailure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        
        var message = new OutboxMessage
        {
            MessageId = Guid.NewGuid(),
            EventType = "2fa.code.generate.sms",
            Payload = "{}",
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
        
        // Act
        message.RetryCount++;
        message.LastError = "Connection failed";
        await context.SaveChangesAsync();
        
        // Assert
        var updated = await context.OutboxMessages
            .FirstOrDefaultAsync(m => m.MessageId == message.MessageId);
        
        Assert.NotNull(updated);
        Assert.Equal(1, updated.RetryCount);
        Assert.Equal("Connection failed", updated.LastError);
    }
}


