namespace PasskeyAuth.Api.Domain.Entities;

public enum InboxMessageStatus
{
    Pending = 1,
    Processed = 2,
    Failed = 3
}

public class InboxMessage
{
    public Guid MessageId { get; set; }
    public string EventId { get; set; } = string.Empty; // max 255, unique - idempotency key
    public string EventType { get; set; } = string.Empty; // max 100
    public string Payload { get; set; } = string.Empty; // JSONB
    public InboxMessageStatus Status { get; set; } = InboxMessageStatus.Pending;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; } = 0;
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

