using System.Net;

namespace PasskeyAuth.Api.Domain.Entities;

public class AuditLog
{
    public Guid LogId { get; set; }
    public Guid? UserId { get; set; }
    public string EventType { get; set; } = string.Empty; // max 100
    public string EventCategory { get; set; } = string.Empty; // max 50
    public string Details { get; set; } = string.Empty; // JSONB
    public IPAddress? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

