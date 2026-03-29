namespace PasskeyAuth.Api.Application.Services;

public interface IEmailService
{
    Task<string> SendCodeAsync(string email, string code);
}


