using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Application.Services;
using PasskeyAuth.Api.Domain.Entities;
using PasskeyAuth.Api.Infrastructure.Data;
using System.Text.Json;

namespace PasskeyAuth.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsersController> _logger;
    private readonly ITwoFactorMethodService _twoFactorMethodService;

    public UsersController(
        ApplicationDbContext context, 
        ILogger<UsersController> logger,
        ITwoFactorMethodService twoFactorMethodService)
    {
        _context = context;
        _logger = logger;
        _twoFactorMethodService = twoFactorMethodService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // Check if user already exists
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existing != null)
            {
                return Conflict(new { error = "User with this email already exists" });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                UserName = request.UserName,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Setup 2FA methods if requested
            var twoFactorMethods = new List<object>();
            if (request.TwoFactorMethods != null && request.TwoFactorMethods.Any())
            {
                foreach (var methodRequest in request.TwoFactorMethods)
                {
                    var setupResult = await _twoFactorMethodService.SetupMethodAsync(
                        user.Id, 
                        methodRequest.MethodType, 
                        methodRequest.Configuration);
                    twoFactorMethods.Add(setupResult);
                }
            }

            _logger.LogInformation("User created. Id: {UserId}, Email: {Email}", user.Id, user.Email);

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName,
                name = user.Name,
                createdAt = user.CreatedAt,
                twoFactorMethods = twoFactorMethods
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            userName = user.UserName,
            name = user.Name,
            createdAt = user.CreatedAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                id = u.Id,
                email = u.Email,
                userName = u.UserName,
                name = u.Name,
                createdAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }
}

public record CreateUserRequest(
    string Email,
    string? UserName = null,
    string? Name = null,
    List<TwoFactorMethodRequest>? TwoFactorMethods = null);

public record TwoFactorMethodRequest(
    TwoFactorMethodType MethodType,
    Dictionary<string, string>? Configuration = null);

