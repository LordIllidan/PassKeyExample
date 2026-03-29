using PasskeyAuth.Api.Domain.Entities;

namespace PasskeyAuth.Api.Application.Services;

public interface ITwoFactorMethodService
{
    Task<string> GenerateCodeAsync(Guid userId, TwoFactorMethodType methodType);
    Task<bool> VerifyCodeAsync(Guid userId, TwoFactorMethodType methodType, string code);
    Task<object> SetupMethodAsync(Guid userId, TwoFactorMethodType methodType, Dictionary<string, string>? configuration = null);
    Task<List<TwoFactorMethod>> GetUserMethodsAsync(Guid userId);
    Task SetPrimaryMethodAsync(Guid userId, Guid methodId);
    Task DisableMethodAsync(Guid userId, Guid methodId);
}
