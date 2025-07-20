using Microsoft.Extensions.Configuration;

namespace PrivateSocial.Tests.Helpers;

public static class TestConfigurationBuilder
{
    public static IConfiguration CreateJwtConfiguration(
        string? secret = null,
        string? issuer = null,
        string? audience = null)
    {
        var settings = new Dictionary<string, string>
        {
            ["JwtSettings:Secret"] = secret ?? "test-secret-key-that-is-at-least-256-bits-long-for-testing",
            ["JwtSettings:Issuer"] = issuer ?? "TestIssuer",
            ["JwtSettings:Audience"] = audience ?? "TestAudience"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();
    }
}