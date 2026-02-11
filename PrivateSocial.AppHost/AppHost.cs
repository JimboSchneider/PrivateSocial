var builder = DistributedApplication.CreateBuilder(args);

// Configure Redis
#pragma warning disable ASPIRECERTIFICATES001
var cache = builder.AddRedis("cache");
if (!builder.ExecutionContext.IsPublishMode)
{
    cache.WithoutHttpsCertificate(); // Disable TLS for local development
}
#pragma warning restore ASPIRECERTIFICATES001

// Get SQL Server password from configuration (with fallback for local dev)
var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sqlServer = builder.AddSqlServer("sql", password: sqlPassword)
    .WithDataVolume()
    .AddDatabase("privatesocial");

// Configure RabbitMQ messaging
var messaging = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin()
    .WithDataVolume();

// Configure API service (port 5475 is set in launchSettings.json)
var apiServiceBuilder = builder.AddProject<Projects.PrivateSocial_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(sqlServer)
    .WaitFor(sqlServer)
    .WithReference(messaging)
    .WaitFor(messaging);

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

builder.AddProject<Projects.PrivateSocial_Worker>("worker")
    .WithReference(messaging)
    .WithReference(sqlServer)
    .WaitFor(messaging);

builder.AddDockerfile("webfrontend", "../PrivateSocial.Web.React")
    .WithHttpEndpoint(port: 3000, targetPort: 80, name: "http")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();