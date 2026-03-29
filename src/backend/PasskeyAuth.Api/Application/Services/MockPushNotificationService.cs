using Microsoft.Extensions.Logging;

namespace PasskeyAuth.Api.Application.Services;

public class MockPushNotificationService : IPushNotificationService
{
    private readonly ILogger<MockPushNotificationService> _logger;

    public MockPushNotificationService(ILogger<MockPushNotificationService> logger)
    {
        _logger = logger;
    }

    public Task<string> SendApprovalRequestAsync(string deviceId, string userId)
    {
        var approvalCode = $"PUSH-{Random.Shared.Next(1000, 9999)}";
        _logger.LogInformation("Mock Push: Approval request {ApprovalCode} would be sent to device {DeviceId} for user {UserId}", 
            approvalCode, deviceId, userId);
        // In real implementation, this would send push notification via FCM, APNS, etc.
        // For tests/mock, we just return the approval code to display
        return Task.FromResult(approvalCode);
    }
}


