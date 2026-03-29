using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Application.Services;
using PasskeyAuth.Api.Infrastructure.Data;

namespace PasskeyAuth.Api.Controllers;

[ApiController]
[Route("api/v1/auth/2fa")]
public class TwoFactorController : ControllerBase
{
    private readonly ITwoFactorService _twoFactorService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TwoFactorController> _logger;

    public TwoFactorController(
        ITwoFactorService twoFactorService,
        ApplicationDbContext context,
        ILogger<TwoFactorController> logger)
    {
        _twoFactorService = twoFactorService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("setup/start")]
    public async Task<IActionResult> StartSetup([FromBody] StartSetupRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var secret = await _twoFactorService.GenerateSecretAsync(request.UserId);
            var qrCodeUri = await _twoFactorService.GenerateQrCodeUriAsync(request.UserId, secret, user.Email);

            return Ok(new
            {
                secret = secret,
                qrCodeUri = qrCodeUri
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting 2FA setup");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("setup/verify")]
    public async Task<IActionResult> VerifySetup([FromBody] VerifySetupRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length != 6 || !request.Code.All(char.IsDigit))
            {
                return BadRequest(new { error = "Code must be 6 digits" });
            }

            await _twoFactorService.EnableTwoFactorAsync(request.UserId, request.Code);
            
            var backupCodes = await _twoFactorService.GenerateBackupCodesAsync(request.UserId);

            return Ok(new
            {
                success = true,
                backupCodes = backupCodes,
                message = "Two-factor authentication enabled successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error verifying 2FA setup");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA setup");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { error = "Code is required" });
            }

            // Code can be 6 digits (TOTP) or 8 digits (backup code)
            if ((request.Code.Length != 6 && request.Code.Length != 8) || !request.Code.All(char.IsDigit))
            {
                return BadRequest(new { error = "Code must be 6 or 8 digits" });
            }

            var isValid = await _twoFactorService.VerifyCodeAsync(request.UserId, request.Code);
            
            if (!isValid)
            {
                // Try backup code
                isValid = await _twoFactorService.VerifyBackupCodeAsync(request.UserId, request.Code);
            }

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

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] DisableRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            await _twoFactorService.DisableTwoFactorAsync(request.UserId);
            return Ok(new { success = true, message = "Two-factor authentication disabled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus([FromQuery] Guid userId)
    {
        try
        {
            // Validation
            if (userId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            var isEnabled = await _twoFactorService.IsTwoFactorEnabledAsync(userId);
            return Ok(new { isEnabled = isEnabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting 2FA status");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("backup-codes/regenerate")]
    public async Task<IActionResult> RegenerateBackupCodes([FromBody] RegenerateBackupCodesRequest request)
    {
        try
        {
            // Validation
            if (request.UserId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid userId" });
            }

            var codes = await _twoFactorService.GenerateBackupCodesAsync(request.UserId);
            return Ok(new { backupCodes = codes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating backup codes");
            return BadRequest(new { error = ex.Message });
        }
    }
}


// Request DTOs
public record StartSetupRequest(Guid UserId);
public record VerifySetupRequest(Guid UserId, string Code);
public record VerifyCodeRequest(Guid UserId, string Code);
public record DisableRequest(Guid UserId);
public record RegenerateBackupCodesRequest(Guid UserId);
