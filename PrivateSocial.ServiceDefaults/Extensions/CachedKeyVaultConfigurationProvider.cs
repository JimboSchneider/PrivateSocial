using System.Text.Json;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PrivateSocial.ServiceDefaults.Extensions;

/// <summary>
/// A configuration provider that caches Azure Key Vault secrets locally for development.
/// </summary>
public class CachedKeyVaultConfigurationProvider : ConfigurationProvider
{
    private readonly Uri _vaultUri;
    private readonly DefaultAzureCredential _credential;
    private readonly string _cacheFilePath;
    private readonly TimeSpan _cacheExpiration;
    private readonly ILogger<CachedKeyVaultConfigurationProvider>? _logger;

    public CachedKeyVaultConfigurationProvider(
        Uri vaultUri, 
        DefaultAzureCredential credential,
        string cacheFilePath,
        TimeSpan cacheExpiration,
        ILogger<CachedKeyVaultConfigurationProvider>? logger = null)
    {
        _vaultUri = vaultUri;
        _credential = credential;
        _cacheFilePath = cacheFilePath;
        _cacheExpiration = cacheExpiration;
        _logger = logger;
    }

    public override void Load()
    {
        // Try to load from cache first
        if (TryLoadFromCache())
        {
            _logger?.LogInformation("Loaded Key Vault secrets from cache");
            return;
        }

        // If cache miss or expired, load from Key Vault
        try
        {
            LoadFromKeyVault();
            SaveToCache();
            _logger?.LogInformation("Loaded Key Vault secrets from Azure and cached locally");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load secrets from Key Vault");
            
            // If we have a cache file (even expired), use it as fallback
            if (File.Exists(_cacheFilePath))
            {
                _logger?.LogWarning("Using expired cache as fallback due to Key Vault connection failure");
                LoadFromCacheFile();
            }
            else
            {
                throw;
            }
        }
    }

    private bool TryLoadFromCache()
    {
        if (!File.Exists(_cacheFilePath))
            return false;

        var fileInfo = new FileInfo(_cacheFilePath);
        if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc > _cacheExpiration)
        {
            _logger?.LogInformation("Cache file expired");
            return false;
        }

        try
        {
            LoadFromCacheFile();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load from cache file");
            return false;
        }
    }

    private void LoadFromCacheFile()
    {
        var json = File.ReadAllText(_cacheFilePath);
        var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        
        if (secrets != null)
        {
            Data = new Dictionary<string, string?>(secrets, StringComparer.OrdinalIgnoreCase);
        }
    }

    private void LoadFromKeyVault()
    {
        var client = new SecretClient(_vaultUri, _credential);
        var secrets = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var secretProperties in client.GetPropertiesOfSecrets())
        {
            if (!secretProperties.Enabled.GetValueOrDefault())
                continue;

            try
            {
                var secret = client.GetSecret(secretProperties.Name);
                secrets[secretProperties.Name] = secret.Value.Value;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to retrieve secret: {SecretName}", secretProperties.Name);
            }
        }

        Data = secrets;
    }

    private void SaveToCache()
    {
        try
        {
            var directory = Path.GetDirectoryName(_cacheFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_cacheFilePath, json);
            
            // Set file permissions to be readable only by the current user
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                File.SetUnixFileMode(_cacheFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save cache file");
            // Don't throw - caching is optional
        }
    }
}

/// <summary>
/// Configuration source for the cached Key Vault provider.
/// </summary>
public class CachedKeyVaultConfigurationSource : IConfigurationSource
{
    public Uri VaultUri { get; set; } = null!;
    public DefaultAzureCredential Credential { get; set; } = null!;
    public string CacheFilePath { get; set; } = null!;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromHours(24);
    public ILogger<CachedKeyVaultConfigurationProvider>? Logger { get; set; }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new CachedKeyVaultConfigurationProvider(
            VaultUri, 
            Credential, 
            CacheFilePath, 
            CacheExpiration,
            Logger);
    }
}