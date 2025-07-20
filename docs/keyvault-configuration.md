# Azure Key Vault Configuration

This document describes how to configure Azure Key Vault for PrivateSocial.

## Required Secrets

The following secrets should be configured in Azure Key Vault:

### 1. JwtSecret
- **Description**: Secret key used for signing JWT tokens
- **Format**: 256-bit (32 byte) secret key
- **Example Generation**:
  ```bash
  # Using OpenSSL
  openssl rand -base64 32
  
  # Add to Key Vault
  az keyvault secret set \
    --vault-name kv-privatesocial-dev \
    --name JwtSecret \
    --value "YOUR_GENERATED_SECRET"
  ```

### 2. DatabaseConnectionString (Production Only)
- **Description**: Full connection string for the production database
- **Format**: SQL Server connection string
- **Examples**:
  
  ```bash
  # For Azure SQL with SQL Authentication
  az keyvault secret set \
    --vault-name kv-privatesocial-dev \
    --name DatabaseConnectionString \
    --value "Server=tcp:your-server.database.windows.net,1433;Database=privatesocial;User ID=your-admin;Password=YourStrongPassword;Encrypt=True;TrustServerCertificate=False;"
  
  # For Azure SQL with Managed Identity (Recommended)
  az keyvault secret set \
    --vault-name kv-privatesocial-dev \
    --name DatabaseConnectionString \
    --value "Server=tcp:your-server.database.windows.net,1433;Database=privatesocial;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;"
  ```

## Local Development

For local development, the application uses:
- JWT Secret from `appsettings.Development.json` (for convenience)
- SQL Server container managed by Aspire with password from `AppHost/appsettings.Development.json`

To use Key Vault in local development:
1. Ensure you're logged in with Azure CLI: `az login`
2. Configure the Key Vault URL in `appsettings.Local.json`
3. Remove local secrets from `appsettings.Development.json`

## Production Deployment

In production:
- All secrets are loaded from Azure Key Vault
- The application uses Managed Identity to access Key Vault
- Database connection uses the full connection string from Key Vault

## Configuration Hierarchy

The application checks for configuration in this order:
1. Azure Key Vault (if configured)
2. User Secrets (local development)
3. appsettings.{Environment}.json
4. appsettings.json
5. Environment variables

## Security Best Practices

1. **Never commit secrets** to source control
2. **Use Managed Identity** for production Azure resources
3. **Rotate secrets regularly** - Update in Key Vault without code changes
4. **Limit Key Vault access** - Use Azure RBAC to control who can read secrets
5. **Monitor access** - Enable Key Vault logging and alerts