using PrivateSocial.ApiService.Data;
using PrivateSocial.ApiService.Extensions;
using PrivateSocial.ApiService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Database
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApiDatabase(builder);

// Add Controllers and Problem Details
builder.Services.AddApiControllers();

// Add Authentication
builder.Services.AddApiAuthentication(builder.Configuration);

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();

// Add OpenAPI
builder.Services.AddApiOpenApi();

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