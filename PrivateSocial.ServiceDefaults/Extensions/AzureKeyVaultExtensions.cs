using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PrivateSocial.ServiceDefaults.Extensions;

/// <summary>
/// Provides extension methods for configuring Azure Key Vault integration.
/// </summary>
public static class AzureKeyVaultExtensions
{
    /// <summary>
    /// Adds Azure Key Vault configuration provider to the application.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddAzureKeyVault(this IHostApplicationBuilder builder)
    {
        var keyVaultEndpoint = builder.Configuration["ConnectionStrings:keyvault"];
        
        if (!string.IsNullOrEmpty(keyVaultEndpoint))
        {
            var keyVaultUri = new Uri(keyVaultEndpoint);
            
            // Use DefaultAzureCredential which works with:
            // - Azure CLI credentials (for local development)
            // - Managed Identity (for Azure deployment)
            // - Visual Studio/VS Code credentials
            // - Environment variables
            var credential = new DefaultAzureCredential();
            
            // Use caching for local development
            if (builder.Environment.IsDevelopment())
            {
                AddCachedKeyVault(builder, keyVaultUri, credential);
            }
            else
            {
                // Use standard Key Vault provider for production
                builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);
            }
        }
        
        return builder;
    }
    
    /// <summary>
    /// Adds Azure Key Vault configuration provider to the application with custom settings.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="keyVaultName">The name of the Azure Key Vault.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static IHostApplicationBuilder AddAzureKeyVault(this IHostApplicationBuilder builder, string keyVaultName)
    {
        if (!string.IsNullOrEmpty(keyVaultName))
        {
            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            var credential = new DefaultAzureCredential();
            
            // Use caching for local development
            if (builder.Environment.IsDevelopment())
            {
                AddCachedKeyVault(builder, keyVaultUri, credential);
            }
            else
            {
                // Use standard Key Vault provider for production
                builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);
            }
        }
        
        return builder;
    }
    
    private static void AddCachedKeyVault(IHostApplicationBuilder builder, Uri keyVaultUri, DefaultAzureCredential credential)
    {
        // Create logger for the configuration provider
        var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var logger = loggerFactory.CreateLogger<CachedKeyVaultConfigurationProvider>();
        
        // Determine cache file path
        var cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PrivateSocial",
            "KeyVaultCache"
        );
        
        var vaultName = keyVaultUri.Host.Split('.')[0];
        var cacheFilePath = Path.Combine(cacheDirectory, $"{vaultName}.cache.json");
        
        // Configure cache expiration (default 24 hours, configurable)
        var cacheExpirationHours = builder.Configuration.GetValue<int>("Azure:KeyVault:CacheExpirationHours", 24);
        var cacheExpiration = TimeSpan.FromHours(cacheExpirationHours);
        
        // Add the cached configuration source
        builder.Configuration.Add(new CachedKeyVaultConfigurationSource
        {
            VaultUri = keyVaultUri,
            Credential = credential,
            CacheFilePath = cacheFilePath,
            CacheExpiration = cacheExpiration,
            Logger = logger
        });
    }
}