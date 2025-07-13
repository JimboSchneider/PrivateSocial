var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var mysql = builder.AddMySql("mysql")
    .WithDataVolume()
    .AddDatabase("privatesocial");

var apiService = builder.AddProject<Projects.PrivateSocial_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(mysql)
    .WaitFor(mysql);

builder.AddNpmApp("webfrontend", "../PrivateSocial.Web.React", "dev")
    .WithHttpEndpoint(targetPort: 3000)
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();