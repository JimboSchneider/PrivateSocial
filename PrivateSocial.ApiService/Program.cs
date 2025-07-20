using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrivateSocial.ApiService.Data;
using PrivateSocial.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add SQL Server with Entity Framework
builder.AddSqlServerDbContext<ApplicationDbContext>("privatesocial");

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new { Field = x.Key, Messages = x.Value!.Errors.Select(e => e.ErrorMessage) })
                .ToList();

            var firstError = errors.FirstOrDefault();
            var message = firstError != null 
                ? firstError.Messages.FirstOrDefault() ?? "Validation failed"
                : "Validation failed";

            return new BadRequestObjectResult(new { message });
        };
    });
builder.Services.AddProblemDetails();

// Add Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// Try to get the secret from configuration first (for local development)
// If not found, try to get it from the secret name (which will be loaded from Key Vault)
var secret = jwtSettings["Secret"];
if (string.IsNullOrEmpty(secret))
{
    var secretName = jwtSettings["SecretName"];
    if (!string.IsNullOrEmpty(secretName))
    {
        // When Key Vault is configured, the secret will be available directly in configuration
        // using the secret name as the key
        secret = builder.Configuration[secretName];
    }
}

if (string.IsNullOrEmpty(secret))
{
    secret = "your-256-bit-secret-key-for-development-only!";
}

var key = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "PrivateSocial",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "PrivateSocialUsers",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new()
        {
            Title = "PrivateSocial API",
            Version = "v1",
            Description = "API for PrivateSocial application"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

// Apply migrations on startup
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Applying database migrations...");
    await dbContext.Database.MigrateAsync();
    logger.LogInformation("Database migrations applied successfully.");
}
catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("already exists"))
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("Database tables already exist. This typically happens when switching from EnsureCreated to Migrations.");
    logger.LogWarning("To fix this, you can either:");
    logger.LogWarning("1. Drop the database and let migrations recreate it");
    logger.LogWarning("2. Add the initial migration to the __EFMigrationsHistory table manually");
    logger.LogWarning("For development, consider running: DROP DATABASE privatesocial; then let EF recreate it.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while applying database migrations.");
}

app.Run();