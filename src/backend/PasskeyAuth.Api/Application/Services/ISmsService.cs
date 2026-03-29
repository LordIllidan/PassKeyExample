namespace PasskeyAuth.Api.Application.Services;

public interface ISmsService
{
    Task<string> SendCodeAsync(string phoneNumber, string code);
}


