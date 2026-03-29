using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PasskeyAuth.Api.Application.Services;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Text;
using System.Text.Json;

namespace PasskeyAuth.Api.Controllers;

[ApiController]
[Route("api/v1/auth/passkey")]
public class PasskeyController : ControllerBase
{
    private readonly IWebAuthnService _webAuthnService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ITwoFactorMethodService _twoFactorMethodService;
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PasskeyController> _logger;
    private readonly IConfiguration _configuration;

    public PasskeyController(
        IWebAuthnService webAuthnService,
        ITwoFactorService twoFactorService,
        ITwoFactorMethodService twoFactorMethodService,
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<PasskeyController> logger,
        IConfiguration configuration)
    {
        _webAuthnService = webAuthnService;
        _twoFactorService = twoFactorService;
        _twoFactorMethodService = twoFactorMethodService;
        _context = context;
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("register/start")]
    public async Task<IActionResult> StartRegistration([FromBody] StartRegistrationRequest request)
    {
        try
        {
            var user = await _webAuthnService.GetUserAsync(request.UserId);
            var challenge = _webAuthnService.GenerateChallenge();

            // Store challenge in cache (expires in 5 minutes)
            var challengeKey = $"passkey:register:challenge:{request.UserId}";
            _cache.Set(challengeKey, challenge, TimeSpan.FromMinutes(5));

            var rpId = _configuration["WebAuthn:RpId"] ?? "localhost";
            var rpName = _configuration["WebAuthn:RpName"] ?? "Passkey Auth";

            // Create PublicKeyCredentialCreationOptions
            var options = new
            {
                challenge = Convert.ToBase64String(challenge),
                rp = new
                {
                    id = rpId,
                    name = rpName
                },
                user = new
                {
                    id = Convert.ToBase64String(user.Id),
                    name = user.Name,
                    displayName = user.DisplayName
                },
                pubKeyCredParams = new[]
                {
                    new { type = "public-key", alg = -7 }, // ES256
                    new { type = "public-key", alg = -257 } // RS256
                },
                authenticatorSelection = new
                {
                    authenticatorAttachment = "platform",
                    userVerification = "preferred",
                    requireResidentKey = true
                },
                timeout = 60000,
                attestation = "none"
            };

            return Ok(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting passkey registration");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("register/finish")]
    public async Task<IActionResult> FinishRegistration([FromBody] FinishRegistrationRequest request)
    {
        try
        {
            // Get challenge from cache
            var challengeKey = $"passkey:register:challenge:{request.UserId}";
            if (!_cache.TryGetValue(challengeKey, out byte[]? challenge))
            {
                return BadRequest(new { error = "Challenge expired or not found" });
            }

            // Parse response
            var responseData = JsonSerializer.Deserialize<JsonElement>(request.Response);
            var credentialId = responseData.GetProperty("rawId").GetString() ?? "";
            var attestationObject = responseData.GetProperty("response").GetProperty("attestationObject").GetString() ?? "";
            var clientDataJSON = responseData.GetProperty("response").GetProperty("clientDataJSON").GetString() ?? "";

            // Basic validation - in production, verify attestation properly
            if (string.IsNullOrEmpty(credentialId) || string.IsNullOrEmpty(attestationObject))
            {
                return BadRequest(new { error = "Invalid credential data" });
            }

            // Extract public key from attestation object (simplified - in production use proper CBOR parsing)
            // For now, we'll store the raw attestation object as public key
            var credentialIdBytes = Convert.FromBase64String(credentialId);
            var publicKeyBytes = Convert.FromBase64String(attestationObject);

            // Store credential
            var deviceType = request.DeviceType ?? "platform";
            var userAgent = Request.Headers["User-Agent"].ToString();
            await _webAuthnService.StoreCredentialAsync(
                request.UserId,
                credentialIdBytes,
                publicKeyBytes,
                0,
                request.Name ?? "My Device",
                deviceType,
                userAgent);

            // Remove challenge from cache
            _cache.Remove(challengeKey);

            var passkeyCredential = await _context.PasskeyCredentials
                .FirstOrDefaultAsync(c => c.CredentialId == credentialId);

            return Ok(new
            {
                id = passkeyCredential!.Id,
                userId = passkeyCredential.UserId,
                name = passkeyCredential.Name,
                deviceType = passkeyCredential.DeviceType,
                createdAt = passkeyCredential.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finishing passkey registration");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login/start")]
    public async Task<IActionResult> StartLogin([FromBody] StartLoginRequest? request = null)
    {
        try
        {
            var challenge = _webAuthnService.GenerateChallenge();

            List<object>? allowedCredentials = null;
            if (!string.IsNullOrEmpty(request?.Email))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user != null)
                {
                    var credentialIds = await _webAuthnService.GetCredentialsByUserIdAsync(user.Id);
                    allowedCredentials = credentialIds.Select(id => new
                    {
                        type = "public-key",
                        id = Convert.ToBase64String(id)
                    }).ToList<object>();
                }
            }

            var challengeBase64 = Convert.ToBase64String(challenge);
            var challengeKey = $"passkey:login:challenge:{challengeBase64}";
            _cache.Set(challengeKey, challenge, TimeSpan.FromMinutes(5));

            var options = new
            {
                challenge = challengeBase64,
                rpId = _configuration["WebAuthn:RpId"] ?? "localhost",
                allowCredentials = allowedCredentials,
                userVerification = "preferred",
                timeout = 60000
            };

            return Ok(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting passkey login");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login/finish")]
    public async Task<IActionResult> FinishLogin([FromBody] FinishLoginRequest request)
    {
        try
        {
            // Get challenge from cache
            var challengeKey = $"passkey:login:challenge:{request.ChallengeBase64}";
            if (!_cache.TryGetValue(challengeKey, out byte[]? cachedChallenge))
            {
                return BadRequest(new { error = "Challenge expired or not found" });
            }

            // Get credential
            var credential = await _webAuthnService.GetCredentialByIdAsync(request.CredentialId);
            if (credential == null)
            {
                return BadRequest(new { error = "Credential not found" });
            }

            // Parse response
            var responseData = JsonSerializer.Deserialize<JsonElement>(request.Response);
            var signature = responseData.GetProperty("response").GetProperty("signature").GetString() ?? "";
            var authenticatorData = responseData.GetProperty("response").GetProperty("authenticatorData").GetString() ?? "";

            // Basic validation - in production, verify signature properly
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(authenticatorData))
            {
                return BadRequest(new { error = "Invalid assertion data" });
            }

            // Update counter (simplified - in production parse from authenticatorData)
            await _webAuthnService.UpdateCounterAsync(
                request.CredentialId,
                credential.Counter + 1);

            // Get user
            var userId = Guid.Parse(Encoding.UTF8.GetString(credential.UserHandle));
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest(new { error = "User not found" });
            }

            // Remove challenge from cache
            _cache.Remove(challengeKey);

            // Check if 2FA is enabled (check both old TwoFactorAuth and new TwoFactorMethods)
            var twoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == user.Id);
            
            var twoFactorMethods = await _context.TwoFactorMethods
                .Where(m => m.UserId == user.Id && m.IsEnabled)
                .ToListAsync();

            var requiresTwoFactor = (twoFactorAuth?.IsEnabled ?? false) || twoFactorMethods.Any();
            var primaryMethod = twoFactorMethods.FirstOrDefault(m => m.IsPrimary) ?? twoFactorMethods.FirstOrDefault();

            // Generate code for display if 2FA is required
            string? generatedCode = null;
            string? codeMessage = null;
            int? methodType = null;
            
            if (requiresTwoFactor)
            {
                try
                {
                    if (primaryMethod != null)
                    {
                        // Use new TwoFactorMethods
                        generatedCode = await _twoFactorMethodService.GenerateCodeAsync(user.Id, primaryMethod.MethodType);
                        methodType = (int)primaryMethod.MethodType;
                        codeMessage = $"Code for {primaryMethod.MethodType} (mock - displayed on screen): {generatedCode}";
                    }
                    else if (twoFactorMethods.Any())
                    {
                        // If methods exist but none is primary, use first one
                        var firstMethod = twoFactorMethods.First();
                        generatedCode = await _twoFactorMethodService.GenerateCodeAsync(user.Id, firstMethod.MethodType);
                        methodType = (int)firstMethod.MethodType;
                        codeMessage = $"Code for {firstMethod.MethodType} (mock - displayed on screen): {generatedCode}";
                    }
                    else if (twoFactorAuth?.IsEnabled == true)
                    {
                        // Fallback to old TOTP method
                        generatedCode = await _twoFactorMethodService.GenerateCodeAsync(user.Id, Domain.Entities.TwoFactorMethodType.TOTP);
                        methodType = (int)Domain.Entities.TwoFactorMethodType.TOTP;
                        codeMessage = $"TOTP code (mock - displayed on screen): {generatedCode}";
                    }
                    
                    // Ensure methodType is set if requiresTwoFactor is true
                    if (!methodType.HasValue)
                    {
                        _logger.LogWarning("requiresTwoFactor is true but methodType is not set. Defaulting to TOTP.");
                        generatedCode = await _twoFactorMethodService.GenerateCodeAsync(user.Id, Domain.Entities.TwoFactorMethodType.TOTP);
                        methodType = (int)Domain.Entities.TwoFactorMethodType.TOTP;
                        codeMessage = $"TOTP code (mock - displayed on screen): {generatedCode}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate 2FA code for display");
                }
            }

            var response = new
            {
                success = true,
                userId = user.Id,
                email = user.Email,
                name = user.Name,
                requiresTwoFactor = requiresTwoFactor,
                twoFactorMethodType = methodType ?? (requiresTwoFactor ? (int?)Domain.Entities.TwoFactorMethodType.TOTP : null),
                generatedCode = generatedCode,
                codeMessage = codeMessage,
                message = requiresTwoFactor 
                    ? "Passkey verified. Please enter your 2FA code." 
                    : "Login successful"
            };
            
            _logger.LogInformation("FinishLogin response: requiresTwoFactor={RequiresTwoFactor}, twoFactorMethodType={MethodType}, generatedCode={GeneratedCode}", 
                requiresTwoFactor, response.twoFactorMethodType, generatedCode);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finishing passkey login");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetPasskeys([FromQuery] Guid userId)
    {
        var credentials = await _context.PasskeyCredentials
            .Where(c => c.UserId == userId)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name,
                deviceType = c.DeviceType,
                createdAt = c.CreatedAt,
                lastUsedAt = c.LastUsedAt
            })
            .ToListAsync();

        return Ok(credentials);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePasskey(Guid id, [FromQuery] Guid userId)
    {
        var credential = await _context.PasskeyCredentials
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (credential == null)
        {
            return NotFound();
        }

        _context.PasskeyCredentials.Remove(credential);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("login/complete")]
    public async Task<IActionResult> CompleteLogin([FromBody] CompleteLoginRequest request)
    {
        try
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return BadRequest(new { error = "User not found" });
            }

            // Check which 2FA method is enabled
            var twoFactorMethods = await _context.TwoFactorMethods
                .Where(m => m.UserId == request.UserId && m.IsEnabled)
                .ToListAsync();
            
            var twoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == request.UserId);

            var primaryMethod = twoFactorMethods.FirstOrDefault(m => m.IsPrimary) ?? twoFactorMethods.FirstOrDefault();
            
            // Verify 2FA code
            bool isValid = false;
            
            if (primaryMethod != null)
            {
                // Use new TwoFactorMethods
                isValid = await _twoFactorMethodService.VerifyCodeAsync(
                    request.UserId, 
                    primaryMethod.MethodType, 
                    request.TwoFactorCode);
            }
            else if (twoFactorAuth?.IsEnabled == true)
            {
                // Fallback to old TOTP method
                isValid = await _twoFactorService.VerifyCodeAsync(request.UserId, request.TwoFactorCode);
                
                if (!isValid)
                {
                    // Try backup code
                    isValid = await _twoFactorService.VerifyBackupCodeAsync(request.UserId, request.TwoFactorCode);
                }
            }

            if (!isValid)
            {
                return BadRequest(new { error = "Invalid 2FA code" });
            }

            return Ok(new
            {
                success = true,
                userId = user.Id,
                email = user.Email,
                name = user.Name,
                message = "Login completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing login with 2FA");
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Request DTOs
public record StartRegistrationRequest(Guid UserId);
public record FinishRegistrationRequest(
    Guid UserId,
    string Response,
    string? Name = null,
    string? DeviceType = null);
public record StartLoginRequest(string? Email = null);
public record FinishLoginRequest(
    string CredentialId,
    string Response,
    string ChallengeBase64);
public record CompleteLoginRequest(
    Guid UserId,
    string TwoFactorCode);
