using System.Net;

namespace PasskeyAuth.Api.Domain.Entities;

public class RateLimitEntry
{
    public Guid EntryId { get; set; }
    public Guid? UserId { get; set; }
    public IPAddress? IpAddress { get; set; }
    public string OperationType { get; set; } = string.Empty; // max 100
    public int AttemptCount { get; set; } = 1;
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public bool IsBlocked { get; set; } = false;
    public DateTime? BlockedUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

