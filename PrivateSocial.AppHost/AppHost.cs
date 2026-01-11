var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

// Get SQL Server password from configuration (with fallback for local dev)
var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sqlServer = builder.AddSqlServer("sql", password: sqlPassword)
    .WithDataVolume()
    .AddDatabase("privatesocial");

// Configure API service (port 5475 is set in launchSettings.json)
var apiServiceBuilder = builder.AddProject<Projects.PrivateSocial_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(sqlServer)
    .WaitFor(sqlServer);

// Add Azure Key Vault configuration
// For local development: Configure Key Vault URL in appsettings or user secrets
var keyVaultUrl = builder.Configuration["Azure:KeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    // For local development with existing Key Vault
    apiServiceBuilder.WithEnvironment("ConnectionStrings__keyvault", keyVaultUrl);
}
else if (builder.ExecutionContext.IsPublishMode)
{
    // For Azure deployment, create Key Vault resource
    var keyVault = builder.AddAzureKeyVault("keyvault");
    apiServiceBuilder.WithReference(keyVault);
}

var apiService = apiServiceBuilder;

builder.AddDockerfile("webfrontend", "../PrivateSocial.Web.React")
    .WithHttpEndpoint(port: 3000, targetPort: 80, name: "http")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();