using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Application.Services;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Text.Json;

namespace PasskeyAuth.Api.Controllers;

[ApiController]
[Route("api/v1/auth/2fa/methods")]
public class TwoFactorMethodController : ControllerBase
{
    private readonly ITwoFactorMethodService _methodService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TwoFactorMethodController> _logger;

    public TwoFactorMethodController(
        ITwoFactorMethodService methodService,
        ApplicationDbContext context,
        ILogger<TwoFactorMethodController> logger)
    {
        _methodService = methodService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateCode([FromBody] GenerateCodeRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            if (!Enum.IsDefined(typeof(TwoFactorMethodType), request.MethodType))
            {
                return BadRequest(new { error = "Invalid methodType" });
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var code = await _methodService.GenerateCodeAsync(request.UserId, request.MethodType);

            // For mock methods, return the code to display on screen
            var response = new
            {
                code = code,
                methodType = request.MethodType.ToString(),
                expiresIn = 300, // 5 minutes
                message = GetMethodMessage(request.MethodType)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating 2FA code");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeMethodRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            if (!Enum.IsDefined(typeof(TwoFactorMethodType), request.MethodType))
            {
                return BadRequest(new { error = "Invalid methodType" });
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { error = "Code is required" });
            }

            var isValid = await _methodService.VerifyCodeAsync(
                request.UserId, 
                request.MethodType, 
                request.Code);

            if (isValid)
            {
                return Ok(new { success = true, message = "Code verified successfully" });
            }

            return BadRequest(new { error = "Invalid code" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserMethods(Guid userId)
    {
        try
        {
            var methods = await _methodService.GetUserMethodsAsync(userId);
            return Ok(methods.Select(m => new
            {
                id = m.Id,
                methodType = m.MethodType.ToString(),
                isEnabled = m.IsEnabled,
                isPrimary = m.IsPrimary,
                configuration = m.Configuration,
                createdAt = m.CreatedAt,
                lastUsedAt = m.LastUsedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user 2FA methods");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("setup")]
    public async Task<IActionResult> SetupMethod([FromBody] SetupMethodRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            if (!Enum.IsDefined(typeof(TwoFactorMethodType), request.MethodType))
            {
                return BadRequest(new { error = "Invalid methodType" });
            }

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var result = await _methodService.SetupMethodAsync(
                request.UserId, 
                request.MethodType, 
                request.Configuration);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up 2FA method");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{methodId}/primary")]
    public async Task<IActionResult> SetPrimary(Guid methodId, [FromBody] SetPrimaryRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            if (methodId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid methodId" });
            }

            var method = await _context.TwoFactorMethods.FindAsync(methodId);
            if (method == null || method.UserId != request.UserId)
            {
                return NotFound(new { error = "Method not found" });
            }

            await _methodService.SetPrimaryMethodAsync(request.UserId, methodId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary method");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{methodId}/disable")]
    public async Task<IActionResult> DisableMethod(Guid methodId, [FromBody] DisableMethodRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            if (methodId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid methodId" });
            }

            var method = await _context.TwoFactorMethods.FindAsync(methodId);
            if (method == null || method.UserId != request.UserId)
            {
                return NotFound(new { error = "Method not found" });
            }

            await _methodService.DisableMethodAsync(request.UserId, methodId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling method");
            return BadRequest(new { error = ex.Message });
        }
    }

    private string GetMethodMessage(TwoFactorMethodType methodType)
    {
        return methodType switch
        {
            TwoFactorMethodType.SMS => "SMS code (mock - displayed on screen)",
            TwoFactorMethodType.Email => "Email code (mock - displayed on screen)",
            TwoFactorMethodType.Push => "Push notification (mock - approval code displayed)",
            TwoFactorMethodType.U2F => "Use your security key (YubiKey, etc.)",
            _ => "Code generated"
        };
    }
}

// Request DTOs
public record GenerateCodeRequest(Guid UserId, TwoFactorMethodType MethodType);
public record VerifyCodeMethodRequest(Guid UserId, TwoFactorMethodType MethodType, string Code);
public record SetupMethodRequest(Guid UserId, TwoFactorMethodType MethodType, Dictionary<string, string>? Configuration = null);
public record SetPrimaryRequest(Guid UserId);
public record DisableMethodRequest(Guid UserId);
