using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using Xunit;

namespace PasskeyAuth.Api.Tests.Domain.Entities;

public class AuthorizationTokenTests
{
    [Fact]
    public void AuthorizationToken_ShouldBeCreated_WithAllRequiredProperties()
    {
        // Arrange
        var challengeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        
        // Act
        var authToken = new AuthorizationToken
        {
            TokenId = Guid.NewGuid(),
            ChallengeId = challengeId,
            UserId = userId,
            OperationType = "user.password.change",
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
        
        // Assert
        Assert.NotEqual(Guid.Empty, authToken.TokenId);
        Assert.Equal(challengeId, authToken.ChallengeId);
        Assert.Equal(userId, authToken.UserId);
        Assert.Equal(token, authToken.Token);
        Assert.False(authToken.IsUsed);
        Assert.Null(authToken.UsedAt);
    }

    [Fact]
    public void AuthorizationToken_ShouldHaveRelationship_WithChallenge()
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
            CreatedAt = DateTime.UtcNow
        };
        
        var token = new AuthorizationToken
        {
            TokenId = Guid.NewGuid(),
            ChallengeId = challenge.ChallengeId,
            UserId = user.Id,
            OperationType = "user.password.change",
            Token = "token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            Challenge = challenge
        };
        
        // Act & Assert
        Assert.NotNull(token.Challenge);
        Assert.Equal(challenge.ChallengeId, token.Challenge.ChallengeId);
    }

    [Fact]
    public async Task AuthorizationToken_ShouldBeSaved_ToDatabase()
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
        
        var token = new AuthorizationToken
        {
            TokenId = Guid.NewGuid(),
            ChallengeId = challenge.ChallengeId,
            UserId = user.Id,
            OperationType = "user.password.change",
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.Users.Add(user);
        context.AuthorizationChallenges.Add(challenge);
        context.AuthorizationTokens.Add(token);
        await context.SaveChangesAsync();
        
        // Assert
        var saved = await context.AuthorizationTokens
            .FirstOrDefaultAsync(t => t.TokenId == token.TokenId);
        
        Assert.NotNull(saved);
        Assert.Equal(token.ChallengeId, saved.ChallengeId);
        Assert.Equal(token.Token, saved.Token);
        Assert.False(saved.IsUsed);
    }

    [Fact]
    public async Task AuthorizationToken_ShouldBeMarkedAsUsed_AfterExecution()
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
        
        var token = new AuthorizationToken
        {
            TokenId = Guid.NewGuid(),
            ChallengeId = challenge.ChallengeId,
            UserId = user.Id,
            OperationType = "user.password.change",
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        context.AuthorizationChallenges.Add(challenge);
        context.AuthorizationTokens.Add(token);
        await context.SaveChangesAsync();
        
        // Act
        token.IsUsed = true;
        token.UsedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        
        // Assert
        var updated = await context.AuthorizationTokens
            .FirstOrDefaultAsync(t => t.TokenId == token.TokenId);
        
        Assert.NotNull(updated);
        Assert.True(updated.IsUsed);
        Assert.NotNull(updated.UsedAt);
    }

    [Fact]
    public async Task AuthorizationToken_ShouldBeDeleted_WhenChallengeIsDeleted()
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
        
        var token = new AuthorizationToken
        {
            TokenId = Guid.NewGuid(),
            ChallengeId = challenge.ChallengeId,
            UserId = user.Id,
            OperationType = "user.password.change",
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
        
        context.Users.Add(user);
        context.AuthorizationChallenges.Add(challenge);
        context.AuthorizationTokens.Add(token);
        await context.SaveChangesAsync();
        
        // Act
        context.AuthorizationChallenges.Remove(challenge);
        await context.SaveChangesAsync();
        
        // Assert
        var deleted = await context.AuthorizationTokens
            .FirstOrDefaultAsync(t => t.TokenId == token.TokenId);
        
        Assert.Null(deleted);
    }

    [Fact]
    public async Task AuthorizationToken_ShouldHaveRelationship_BetweenChallengeAndToken()
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
        
        var token = new AuthorizationToken
        {
            TokenId = Guid.NewGuid(),
            ChallengeId = challenge.ChallengeId,
            UserId = user.Id,
            OperationType = "user.password.change",
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            CreatedAt = DateTime.UtcNow
        };
        
        // Act
        context.Users.Add(user);
        context.AuthorizationChallenges.Add(challenge);
        context.AuthorizationTokens.Add(token);
        await context.SaveChangesAsync();
        
        // Assert
        var savedChallenge = await context.AuthorizationChallenges
            .Include(c => c.AuthorizationToken)
            .FirstOrDefaultAsync(c => c.ChallengeId == challenge.ChallengeId);
        
        Assert.NotNull(savedChallenge);
        Assert.NotNull(savedChallenge.AuthorizationToken);
        Assert.Equal(token.TokenId, savedChallenge.AuthorizationToken.TokenId);
    }
}


