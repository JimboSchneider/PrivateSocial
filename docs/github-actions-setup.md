# GitHub Actions Setup

This document describes the GitHub secrets, variables, and workflows for the CI/CD pipelines.

## Workflow Structure

The repository uses a modular workflow approach for PR validation:

### Main Workflows

1. **`pr-validation.yml`** - Orchestrates all PR checks
   - Triggers on PRs to `main` branch
   - Detects changes and runs relevant validations
   - Provides consolidated status checks

2. **`backend-validation.yml`** - .NET validation
   - Builds and tests .NET solution
   - Runs unit tests with coverage
   - Checks for build warnings

3. **`frontend-validation.yml`** - React validation
   - ESLint and TypeScript checks
   - Builds React application
   - Runs unit tests

4. **`e2e-validation.yml`** - End-to-end testing
   - Spins up SQL Server and Redis
   - Runs Playwright tests
   - Captures screenshots on failure

5. **`claude-code-review.yml`** - Automated code review
   - Runs on PR open/update
   - Provides AI-powered code review
   - Uses sticky comments for updates

6. **`claude.yml`** - Interactive Claude assistant
   - Responds to @claude mentions
   - Helps with code questions in PRs/issues

## Required GitHub Secrets

These secrets should be configured at the repository level in Settings → Secrets and variables → Actions.

### CI/CD Secrets

1. **CLAUDE_CODE_OAUTH_TOKEN**
   - Description: OAuth token for Claude Code GitHub integration
   - Required for: Claude code review and interactive workflows
   - Get from: Claude Code GitHub integration setup

## GitHub Variables (Optional - for future Azure deployment)

These variables would be configured at the repository level in Settings → Secrets and variables → Actions → Variables tab when Azure deployment is needed.

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

## Setting Up PR Checks

To enable PR validation checks:

1. Go to Settings → Branches
2. Add a branch protection rule for `main`
3. Enable "Require status checks to pass before merging"
4. Select these status checks:
   - `PR Validation Status`
   - `Backend Validation` (if available)
   - `Frontend Validation` (if available)
   - `E2E Tests` (if available)

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