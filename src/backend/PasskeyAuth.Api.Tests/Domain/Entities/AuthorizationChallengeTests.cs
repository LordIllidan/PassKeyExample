using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using Xunit;

namespace PasskeyAuth.Api.Tests.Domain.Entities;

public class AuthorizationChallengeTests
{
    [Fact]
    public void AuthorizationChallenge_ShouldBeCreated_WithAllRequiredProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var operationType = "user.password.change";
        var operationData = "{\"newPassword\":\"hashed\"}";
        
        // Act
        var challenge = new AuthorizationChallenge
        {
            ChallengeId = Guid.NewGuid(),
            UserId = userId,
            OperationType = operationType,
            OperationData = operationData,
            Status = AuthorizationChallengeStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, challenge.ChallengeId);
        Assert.Equal(userId, challenge.UserId);
        Assert.Equal(operationType, challenge.OperationType);
        Assert.Equal(operationData, challenge.OperationData);
        Assert.Equal(AuthorizationChallengeStatus.Pending, challenge.Status);
        Assert.Equal(0, challenge.FailedAttempts);
        Assert.Null(challenge.VerifiedAt);
    }

    [Fact]
    public void AuthorizationChallenge_ShouldHaveRelationship_WithUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow
        };
        
        var challenge = new AuthorizationChallenge
        {
            ChallengeId = Guid.NewGuid(),
            UserId = user.Id,
            OperationType = "user.password.change",
            OperationData = "{}",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            User = user
        };
        
        // Act & Assert
        Assert.NotNull(challenge.User);
        Assert.Equal(user.Id, challenge.User.Id);
        Assert.Equal(user.Email, challenge.User.Email);
    }

    [Fact]
    public async Task AuthorizationChallenge_ShouldBeSaved_ToDatabase()
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
        
        var challenge = new AuthorizationChallenge
        {
            ChallengeId = Guid.NewGuid(),
            UserId = user.Id,
            OperationType = "user.password.change",
            OperationData = "{\"newPassword\":\"hashed\"}",
            Status = AuthorizationChallengeStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.Users.Add(user);
        context.AuthorizationChallenges.Add(challenge);
        await context.SaveChangesAsync();
        
        // Assert
        var saved = await context.AuthorizationChallenges
            .FirstOrDefaultAsync(c => c.ChallengeId == challenge.ChallengeId);
        
        Assert.NotNull(saved);
        Assert.Equal(challenge.UserId, saved.UserId);
        Assert.Equal(challenge.OperationType, saved.OperationType);
        Assert.Equal(challenge.Status, saved.Status);
    }

    [Fact]
    public async Task AuthorizationChallenge_ShouldTransition_FromPendingToVerified()
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
        
        var challenge = new AuthorizationChallenge
        {
            ChallengeId = Guid.NewGuid(),
            UserId = user.Id,
            OperationType = "user.password.change",
            OperationData = "{}",
            Status = AuthorizationChallengeStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        context.AuthorizationChallenges.Add(challenge);
        await context.SaveChangesAsync();
        
        // Act
        challenge.Status = AuthorizationChallengeStatus.Verified;
        challenge.VerifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        
        // Assert
        var updated = await context.AuthorizationChallenges
            .FirstOrDefaultAsync(c => c.ChallengeId == challenge.ChallengeId);
        
        Assert.NotNull(updated);
        Assert.Equal(AuthorizationChallengeStatus.Verified, updated.Status);
        Assert.NotNull(updated.VerifiedAt);
    }

    [Fact]
    public async Task AuthorizationChallenge_ShouldExpire_AfterTimeout()
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
        
        var challenge = new AuthorizationChallenge
        {
            ChallengeId = Guid.NewGuid(),
            UserId = user.Id,
            OperationType = "user.password.change",
            OperationData = "{}",
            Status = AuthorizationChallengeStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1), // Already expired
            CreatedAt = DateTime.UtcNow.AddMinutes(-6)
        };
        
        context.Users.Add(user);
        context.AuthorizationChallenges.Add(challenge);
        await context.SaveChangesAsync();
        
        // Act
        if (challenge.ExpiresAt < DateTime.UtcNow && challenge.Status == AuthorizationChallengeStatus.Pending)
        {
            challenge.Status = AuthorizationChallengeStatus.Expired;
            await context.SaveChangesAsync();
        }
        
        // Assert
        var expired = await context.AuthorizationChallenges
            .FirstOrDefaultAsync(c => c.ChallengeId == challenge.ChallengeId);
        
        Assert.NotNull(expired);
        Assert.Equal(AuthorizationChallengeStatus.Expired, expired.Status);
    }

    [Fact]
    public async Task AuthorizationChallenge_ShouldBeDeleted_WhenUserIsDeleted()
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
        
        var challenge = new AuthorizationChallenge
        {
            ChallengeId = Guid.NewGuid(),
            UserId = user.Id,
            OperationType = "user.password.change",
            OperationData = "{}",
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        context.AuthorizationChallenges.Add(challenge);
        await context.SaveChangesAsync();
        
        // Act
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        
        // Assert
        var deleted = await context.AuthorizationChallenges
            .FirstOrDefaultAsync(c => c.ChallengeId == challenge.ChallengeId);
        
        Assert.Null(deleted);
    }
}


