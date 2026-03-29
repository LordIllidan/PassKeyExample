namespace PasskeyAuth.Api.Application.Services;

public interface IPushNotificationService
{
    Task<string> SendApprovalRequestAsync(string deviceId, string userId);
}


