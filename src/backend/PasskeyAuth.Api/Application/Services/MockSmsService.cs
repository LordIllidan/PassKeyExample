using Microsoft.Extensions.Logging;

namespace PasskeyAuth.Api.Application.Services;

public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task<string> SendCodeAsync(string phoneNumber, string code)
    {
        _logger.LogInformation("Mock SMS: Code {Code} would be sent to {PhoneNumber}", code, phoneNumber);
        // In real implementation, this would send SMS via Twilio, AWS SNS, etc.
        // For tests/mock, we just return the code to display
        return Task.FromResult(code);
    }
}


