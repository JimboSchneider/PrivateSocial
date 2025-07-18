var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var mysql = builder.AddMySql("mysql")
    .WithDataVolume()
    .AddDatabase("privatesocial");

// Configure API service
var apiServiceBuilder = builder.AddProject<Projects.PrivateSocial_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(mysql)
    .WaitFor(mysql);

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
    .WithHttpEndpoint(targetPort: 80, name: "http")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();