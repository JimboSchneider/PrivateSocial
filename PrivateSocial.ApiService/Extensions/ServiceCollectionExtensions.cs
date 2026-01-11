using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrivateSocial.ApiService.Data;

namespace PrivateSocial.ApiService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

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
                secret = configuration[secretName];
            }
        }

        if (string.IsNullOrEmpty(secret))
        {
            secret = "your-256-bit-secret-key-for-development-only!";
        }

        var key = Encoding.ASCII.GetBytes(secret);

        services.AddAuthentication(options =>
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

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi(options =>
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

        return services;
    }

    public static IServiceCollection AddApiDatabase(this IServiceCollection services, IHostApplicationBuilder builder)
    {
        // Add SQL Server with Entity Framework
        // Check if we have a connection string from Key Vault first
        var connectionString = builder.Configuration["DatabaseConnectionString"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            // Use connection string from Key Vault (production scenario)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
        else
        {
            // Use Aspire service discovery (local development with containers)
            builder.AddSqlServerDbContext<ApplicationDbContext>("privatesocial");
        }

        return services;
    }
    
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers()
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
        
        services.AddProblemDetails();
        
        return services;
    }
}
