# Database Configuration

This document describes how to configure database connections securely for the PrivateSocial application.

## Overview

The application uses MySQL database with Entity Framework Core. Database credentials should **never** be hardcoded in source files. Instead, use one of the following secure configuration methods.

## Configuration Methods

### 1. User Secrets (Development Only)

For local development, use .NET User Secrets to store sensitive data outside of your project tree:

```bash
cd PrivateSocial.ApiService
dotnet user-secrets set "ConnectionStrings:privatesocial" "Server=localhost;Port=3306;Database=privatesocial_dev;User=myuser;Password=mypassword;"
```

### 2. Environment Variables

Set the connection string using environment variables:

```bash
# Linux/macOS
export PRIVATESOCIAL_DB_CONNECTION="Server=localhost;Port=3306;Database=privatesocial_dev;User=myuser;Password=mypassword;"

# Windows Command Prompt
set PRIVATESOCIAL_DB_CONNECTION=Server=localhost;Port=3306;Database=privatesocial_dev;User=myuser;Password=mypassword;

# Windows PowerShell
$env:PRIVATESOCIAL_DB_CONNECTION="Server=localhost;Port=3306;Database=privatesocial_dev;User=myuser;Password=mypassword;"
```

### 3. appsettings.json (Non-sensitive environments only)

For non-production environments, you can add to `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "privatesocial": "Server=localhost;Port=3306;Database=privatesocial_dev;User=myuser;Password=mypassword;"
  }
}
```

**Note**: Never commit passwords to source control. Use this method only with non-sensitive test databases.

### 4. Azure Key Vault (Production)

For production deployments, the connection string can be stored in Azure Key Vault:

```bash
az keyvault secret set --vault-name kv-privatesocial-dev --name ConnectionStrings--privatesocial --value "Server=myserver;Port=3306;Database=privatesocial;User=myuser;Password=mypassword;"
```

## Configuration Priority

The application checks for database configuration in the following order:

1. `ConnectionStrings:privatesocial` from configuration (User Secrets, appsettings.json, Key Vault)
2. `PRIVATESOCIAL_DB_CONNECTION` environment variable
3. If neither is found, an error is thrown with helpful instructions

## Entity Framework Migrations

When running EF Core migrations, the `ApplicationDbContextFactory` will use the same configuration sources:

```bash
# Add a new migration
dotnet ef migrations add MigrationName --project PrivateSocial.ApiService

# Update the database
dotnet ef database update --project PrivateSocial.ApiService
```

## Connection String Format

MySQL connection strings should follow this format:

```
Server=<hostname>;Port=<port>;Database=<database>;User=<username>;Password=<password>;
```

Additional options can be added as needed:
- `SslMode=Required` - For SSL connections
- `AllowPublicKeyRetrieval=True` - For certain authentication scenarios
- `ConnectionTimeout=30` - Connection timeout in seconds

## Security Best Practices

1. **Never commit credentials** to source control
2. **Use strong passwords** with mixed case, numbers, and special characters
3. **Rotate credentials regularly**
4. **Use least privilege** - Database users should have only necessary permissions
5. **Enable SSL/TLS** for production database connections
6. **Use Managed Identity** when deploying to Azure (eliminates need for passwords)

## Troubleshooting

### "Database connection string not found" error

This error occurs when no configuration is found. Check:

1. User secrets are set correctly: `dotnet user-secrets list`
2. Environment variable is set: `echo $PRIVATESOCIAL_DB_CONNECTION` (Linux/macOS) or `echo %PRIVATESOCIAL_DB_CONNECTION%` (Windows)
3. You're in the correct directory when running commands

### "Access denied for user" error

1. Verify username and password are correct
2. Ensure the database user has appropriate permissions
3. Check if the MySQL server allows connections from your host

### SSL/TLS connection errors

Add `SslMode=None` for local development (not recommended for production):

```
Server=localhost;Port=3306;Database=privatesocial_dev;User=myuser;Password=mypassword;SslMode=None;
```