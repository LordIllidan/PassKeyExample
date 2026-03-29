using Microsoft.Extensions.Logging;

namespace PasskeyAuth.Api.Application.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task<string> SendCodeAsync(string email, string code)
    {
        _logger.LogInformation("Mock Email: Code {Code} would be sent to {Email}", code, email);
        // In real implementation, this would send email via SMTP, SendGrid, etc.
        // For tests/mock, we just return the code to display
        return Task.FromResult(code);
    }
}


