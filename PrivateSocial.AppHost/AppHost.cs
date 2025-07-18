var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var mysql = builder.AddMySql("mysql")
    .WithDataVolume()
    .AddDatabase("privatesocial");

var apiService = builder.AddProject<Projects.PrivateSocial_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(mysql)
    .WaitFor(mysql);

builder.AddDockerfile("webfrontend", "../PrivateSocial.Web.React")
    .WithHttpEndpoint(targetPort: 80, name: "http")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();