using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Domain.Entities;
using System.Text.Json;

namespace PasskeyAuth.Api.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<PasskeyCredential> PasskeyCredentials { get; set; }
    public DbSet<TwoFactorAuth> TwoFactorAuths { get; set; }
    public DbSet<TwoFactorMethod> TwoFactorMethods { get; set; }
    public DbSet<AuthorizationChallenge> AuthorizationChallenges { get; set; }
    public DbSet<AuthorizationToken> AuthorizationTokens { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<RateLimitEntry> RateLimitEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users", "auth");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // PasskeyCredential configuration
        modelBuilder.Entity<PasskeyCredential>(entity =>
        {
            entity.ToTable("passkey_credentials", "auth");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CredentialId).IsRequired();
            entity.Property(e => e.PublicKey).IsRequired();
            entity.HasIndex(e => e.CredentialId).IsUnique();
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.PasskeyCredentials)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TwoFactorAuth configuration
        modelBuilder.Entity<TwoFactorAuth>(entity =>
        {
            entity.ToTable("two_factor_auths", "auth");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.SecretKey).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.UserId).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithOne(u => u.TwoFactorAuth)
                .HasForeignKey<TwoFactorAuth>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TwoFactorMethod configuration
        modelBuilder.Entity<TwoFactorMethod>(entity =>
        {
            entity.ToTable("two_factor_methods", "auth");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.MethodType).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsPrimary })
                .HasFilter("\"IsPrimary\" = true")
                .HasDatabaseName("idx_two_factor_methods_user_primary");
            entity.HasIndex(e => new { e.UserId, e.IsEnabled })
                .HasFilter("\"IsEnabled\" = true")
                .HasDatabaseName("idx_two_factor_methods_enabled");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.TwoFactorMethods)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuthorizationChallenge configuration
        modelBuilder.Entity<AuthorizationChallenge>(entity =>
        {
            entity.ToTable("authorization_challenges", "auth");
            entity.HasKey(e => e.ChallengeId);
            entity.Property(e => e.ChallengeId).ValueGeneratedNever();
            entity.Property(e => e.OperationType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OperationData).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.ChallengeCode).HasMaxLength(10);
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.Status, e.ExpiresAt });
            entity.HasIndex(e => e.ExpiresAt).HasFilter("\"Status\" = 1"); // Pending status
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuthorizationToken configuration
        modelBuilder.Entity<AuthorizationToken>(entity =>
        {
            entity.ToTable("authorization_tokens", "auth");
            entity.HasKey(e => e.TokenId);
            entity.Property(e => e.TokenId).ValueGeneratedNever();
            entity.Property(e => e.OperationType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Token).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ChallengeId);
            entity.HasIndex(e => e.ExpiresAt).HasFilter("\"IsUsed\" = false");
            entity.HasIndex(e => e.Token).IsUnique().HasFilter("\"IsUsed\" = false");
            
            entity.HasOne(e => e.Challenge)
                .WithOne(c => c.AuthorizationToken)
                .HasForeignKey<AuthorizationToken>(e => e.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OutboxMessage configuration
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages", "auth");
            entity.HasKey(e => e.MessageId);
            entity.Property(e => e.MessageId).ValueGeneratedNever();
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Payload).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => new { e.Status, e.CreatedAt })
                .HasFilter("\"Status\" IN (1, 2)"); // Pending or Published
            entity.HasIndex(e => new { e.RetryCount, e.CreatedAt })
                .HasFilter("\"Status\" = 1 AND \"RetryCount\" < 3"); // Pending with retry count
        });

        // InboxMessage configuration
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages", "auth");
            entity.HasKey(e => e.MessageId);
            entity.Property(e => e.MessageId).ValueGeneratedNever();
            entity.Property(e => e.EventId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Payload).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.Status).IsRequired();
            entity.HasIndex(e => e.EventId).IsUnique(); // Idempotency key
            entity.HasIndex(e => new { e.Status, e.CreatedAt })
                .HasFilter("\"Status\" = 1"); // Pending
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs", "auth");
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.LogId).ValueGeneratedNever();
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EventCategory).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Details).IsRequired().HasColumnType("jsonb");
            entity.Property(e => e.Success).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.EventType, e.CreatedAt });
            entity.HasIndex(e => new { e.EventCategory, e.CreatedAt });
            entity.HasIndex(e => e.CreatedAt).IsDescending();
        });

        // RateLimitEntry configuration
        modelBuilder.Entity<RateLimitEntry>(entity =>
        {
            entity.ToTable("rate_limit_entries", "auth");
            entity.HasKey(e => e.EntryId);
            entity.Property(e => e.EntryId).ValueGeneratedNever();
            entity.Property(e => e.OperationType).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.UserId, e.OperationType, e.WindowStart });
            entity.HasIndex(e => new { e.IpAddress, e.OperationType, e.WindowStart });
            entity.HasIndex(e => e.BlockedUntil)
                .HasFilter("\"IsBlocked\" = true");
        });
    }
}

