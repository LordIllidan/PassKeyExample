using Fido2NetLib;
using Microsoft.EntityFrameworkCore;
using PasskeyAuth.Api.Application.Services;
using PasskeyAuth.Api.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4400", "https://localhost:4443", "http://frontend", "https://frontend")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("PasskeyAuth.Api")));

// WebAuthn
builder.Services.AddScoped<IWebAuthnService, WebAuthnService>();

// Two-Factor Authentication
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<ITwoFactorMethodService, TwoFactorMethodService>();

// External services for 2FA (mock implementations for tests)
builder.Services.AddScoped<ISmsService, MockSmsService>();
builder.Services.AddScoped<IEmailService, MockEmailService>();
builder.Services.AddScoped<IPushNotificationService, MockPushNotificationService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
}

// HTTPS redirection disabled in Docker - Nginx handles HTTPS
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health");

app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

Log.Information("Application started");

app.Run();


