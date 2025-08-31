# GitHub Actions Setup

This document describes the GitHub secrets and variables required for the CI/CD pipelines.

## Required GitHub Secrets

These secrets should be configured at the repository level in Settings → Secrets and variables → Actions.

### CI/CD Secrets

1. **CI_SQL_PASSWORD**
   - Description: SQL Server SA password for CI/CD database
   - Example: `YourStrong@Passw0rd`
   - Used in: CI and E2E test workflows

2. **CI_DATABASE_CONNECTION_STRING**
   - Description: Full connection string for CI/CD SQL Server
   - Example: `Server=localhost,1433;Database=privatesocial;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True`
   - Used in: CI and E2E test workflows

3. **CI_JWT_SECRET**
   - Description: JWT signing key for CI/CD environment
   - Example: Generate with `openssl rand -base64 32`
   - Used in: CI and E2E test workflows

### Azure Deployment Secrets

4. **AZURE_CACHE_PASSWORD**
   - Description: Redis cache password for Azure deployment
   - Used in: Azure deployment workflow

## Required GitHub Variables

These variables should be configured at the repository level in Settings → Secrets and variables → Actions → Variables tab.

### Azure Deployment Variables

1. **AZURE_CLIENT_ID**
   - Description: Azure Service Principal Client ID
   - Used for: GitHub OIDC authentication with Azure

2. **AZURE_TENANT_ID**
   - Description: Azure AD Tenant ID
   - Used for: GitHub OIDC authentication with Azure

3. **AZURE_SUBSCRIPTION_ID**
   - Description: Azure Subscription ID
   - Used for: Identifying which Azure subscription to deploy to

4. **AZURE_ENV_NAME**
   - Description: Azure environment name (e.g., `dev`, `staging`, `prod`)
   - Used for: Resource naming and tagging

5. **AZURE_LOCATION**
   - Description: Azure region for deployment (e.g., `eastus`, `westus2`)
   - Used for: Specifying where to deploy resources

6. **AZURE_RESOURCE_GROUP**
   - Description: Azure Resource Group name
   - Used for: Container Apps deployment

7. **AZURE_CONTAINER_APP_ENV**
   - Description: Azure Container Apps Environment name
   - Used for: Container Apps deployment

## Environment-Specific Configuration

### Staging Environment
- Create an environment named `staging` in GitHub
- Add protection rules as needed (e.g., required reviewers)
- Can override any of the above secrets/variables at the environment level

### Production Environment
- Create an environment named `production` in GitHub
- Add protection rules:
  - Required reviewers
  - Deployment branches: only `main`
  - Wait timer (optional)
- Override production-specific secrets/variables

## Local Development

For local development, you can create a `.env` file (not committed) with:

```bash
# .env.local
export CI_SQL_PASSWORD="YourLocalPassword"
export CI_DATABASE_CONNECTION_STRING="Server=localhost,1433;Database=privatesocial_dev;User Id=sa;Password=YourLocalPassword;TrustServerCertificate=True"
export CI_JWT_SECRET="your-local-jwt-secret"
```

## Security Best Practices

1. **Rotate secrets regularly** - Update GitHub secrets quarterly
2. **Use strong passwords** - Follow SQL Server password requirements
3. **Limit access** - Only give repository admin access to those who need it
4. **Audit usage** - Review GitHub Actions logs regularly
5. **Use environments** - Separate staging and production secrets

## Troubleshooting

### Common Issues

1. **SQL Server connection fails in CI**
   - Check that `CI_SQL_PASSWORD` matches the password in `CI_DATABASE_CONNECTION_STRING`
   - Ensure the password meets SQL Server complexity requirements

2. **JWT authentication fails**
   - Verify `CI_JWT_SECRET` is at least 32 characters
   - Check that the same secret is used across all services

3. **Azure deployment fails**
   - Verify all Azure variables are set correctly
   - Check that the Service Principal has the correct permissions
   - Ensure OIDC is configured between GitHub and Azure