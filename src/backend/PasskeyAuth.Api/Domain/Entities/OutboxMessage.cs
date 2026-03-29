namespace PasskeyAuth.Api.Domain.Entities;

public enum OutboxMessageStatus
{
    Pending = 1,
    Published = 2,
    Processed = 3
}

public class OutboxMessage
{
    public Guid MessageId { get; set; }
    public string EventType { get; set; } = string.Empty; // max 100
    public string Payload { get; set; } = string.Empty; // JSONB
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

