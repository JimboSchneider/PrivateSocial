# Azure Key Vault Setup

This document describes how Azure Key Vault is integrated into the PrivateSocial application for secure secret management.

## Overview

The application uses Azure Key Vault to securely store sensitive configuration values such as JWT secrets. The integration supports both production environments and local development with intelligent caching for improved developer experience.

## Architecture

### Key Components

1. **ServiceDefaults** - Contains the `AzureKeyVaultExtensions` class that adds Key Vault configuration provider
2. **AppHost** - Declares the Key Vault resource using `AddAzureKeyVault("keyvault")`
3. **API Service** - Consumes secrets from Key Vault through the configuration system

### How It Works

1. When deployed to Azure, the Key Vault resource is provisioned
2. The application uses `DefaultAzureCredential` for authentication:
   - **Local Development**: Uses Azure CLI, Visual Studio, or VS Code credentials
   - **Azure Deployment**: Uses Managed Identity
3. Secrets from Key Vault are loaded into the configuration system
4. Application code accesses secrets through `IConfiguration` as normal

## Configuration

### AppSettings Structure

**appsettings.json** (Production):
```json
{
  "JwtSettings": {
    "SecretName": "JwtSecret",
    "Issuer": "PrivateSocial",
    "Audience": "PrivateSocialUsers"
  }
}
```

**appsettings.Development.json** (Local Development):
```json
{
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key-for-development-only!",
    "SecretName": "JwtSecret",
    "Issuer": "PrivateSocial",
    "Audience": "PrivateSocialUsers"
  }
}
```

### Secret Resolution Logic

The application uses the following logic to resolve the JWT secret:

1. First, check for `JwtSettings:Secret` in configuration (local development)
2. If not found, use `JwtSettings:SecretName` to look up the secret from Key Vault
3. If still not found, fall back to a default development secret

## Azure Deployment

### Prerequisites

1. Azure subscription
2. Azure CLI installed and authenticated
3. Azure Developer CLI (`azd`) installed

### Deployment Steps

1. **Initialize Azure resources**:
   ```bash
   azd init
   ```

2. **Deploy to Azure**:
   ```bash
   azd up
   ```

3. **Add secrets to Key Vault**:
   ```bash
   # Get the Key Vault name from Azure Portal or azd output
   az keyvault secret set --vault-name <your-keyvault-name> --name JwtSecret --value "<your-secure-jwt-secret>"
   ```

### Generating a Secure JWT Secret

Generate a secure 256-bit secret:

```bash
# Using OpenSSL
openssl rand -base64 32

# Using PowerShell
[Convert]::ToBase64String((1..32 | ForEach {Get-Random -Minimum 0 -Maximum 256}))
```

## Local Development

The application now supports using Azure Key Vault in local development with intelligent caching to minimize API calls and improve performance.

### Setting Up Local Development with Key Vault

1. **Create an Azure Key Vault** (if you don't have one):
   ```bash
   # Create a resource group
   az group create --name rg-privatesocial-dev --location eastus
   
   # Create a Key Vault
   az keyvault create --name kv-privatesocial-dev --resource-group rg-privatesocial-dev --location eastus
   ```

2. **Add your secrets to Key Vault**:
   ```bash
   # Add the JWT secret
   az keyvault secret set --vault-name kv-privatesocial-dev --name JwtSecret --value "your-secure-jwt-secret"
   ```

3. **Authenticate with Azure CLI**:
   ```bash
   az login
   ```

4. **Configure the Key Vault URL**:
   
   **Option 1: Using appsettings.Local.json** (recommended):
   ```json
   {
     "Azure": {
       "KeyVault": {
         "VaultUri": "https://kv-privatesocial-dev.vault.azure.net/",
         "CacheExpirationHours": 24
       }
     }
   }
   ```
   
   **Option 2: Using User Secrets**:
   ```bash
   cd PrivateSocial.AppHost
   dotnet user-secrets set "Azure:KeyVault:VaultUri" "https://kv-privatesocial-dev.vault.azure.net/"
   ```

### Caching Mechanism

The application implements intelligent caching for local development:

- **Cache Location**: `~/Library/Application Support/PrivateSocial/KeyVaultCache/` (macOS) or equivalent on other platforms
- **Cache Duration**: 24 hours by default (configurable via `Azure:KeyVault:CacheExpirationHours`)
- **Fallback**: If Key Vault is unavailable but cache exists, the cached values are used
- **Security**: Cache files are created with restricted permissions (readable only by current user)

### Benefits of Caching

1. **Faster Startup**: After initial load, subsequent starts use cached values
2. **Offline Development**: Continue working even without internet/VPN connection
3. **Reduced API Calls**: Minimizes calls to Azure Key Vault API
4. **Cost Efficiency**: Reduces transaction costs on Key Vault

### Cache Management

To clear the cache and force a refresh:
```bash
# macOS/Linux
rm -rf ~/Library/Application\ Support/PrivateSocial/KeyVaultCache/

# Windows
Remove-Item -Path "$env:LOCALAPPDATA\PrivateSocial\KeyVaultCache" -Recurse
```

### Development Without Key Vault

If you prefer not to use Key Vault for local development, simply don't configure the `Azure:KeyVault:VaultUri` setting. The application will fall back to using the JWT secret from `appsettings.Development.json`.

## Security Best Practices

1. **Never commit secrets** to source control
2. **Use strong secrets** - at least 256 bits of entropy
3. **Rotate secrets regularly** - update in Key Vault without code changes
4. **Limit access** - use RBAC to control who can access Key Vault
5. **Enable soft delete** on Key Vault for recovery options

## Troubleshooting

### Common Issues

1. **"JWT Secret not configured" error**
   - Ensure the secret exists in Key Vault
   - Check that the secret name in appsettings matches the Key Vault secret name
   - Verify Azure authentication is working

2. **Authentication failures**
   - For local development: Run `az login`
   - For Azure: Ensure Managed Identity is enabled and has Key Vault access

3. **Key Vault not found**
   - Check the connection string format: `https://<name>.vault.azure.net/`
   - Ensure the Key Vault exists and is accessible

## Adding Additional Secrets

To add more secrets:

1. **Update the configuration model** in appsettings.json
2. **Add the secret to Key Vault**:
   ```bash
   az keyvault secret set --vault-name <vault-name> --name <secret-name> --value "<secret-value>"
   ```
3. **Access in code**:
   ```csharp
   var secretValue = Configuration["SecretName"];
   ```

## References

- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [.NET Aspire Azure Integration](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/integrations)
- [DefaultAzureCredential](https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential)