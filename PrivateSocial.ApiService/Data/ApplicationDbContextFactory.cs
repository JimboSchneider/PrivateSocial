using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PrivateSocial.ApiService.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Build configuration from multiple sources
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<ApplicationDbContextFactory>(optional: true)
            .Build();
        
        // Try to get connection string from configuration first
        var connectionString = configuration.GetConnectionString("privatesocial");
        
        // Fall back to environment variable if not found
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("PRIVATESOCIAL_DB_CONNECTION");
        }
        
        // If still not found, provide helpful error message
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not found. " +
                "Please set either:" +
                "\n1. ConnectionStrings:privatesocial in appsettings.json or user secrets" +
                "\n2. PRIVATESOCIAL_DB_CONNECTION environment variable" +
                "\n\nExample: Server=localhost;Port=3306;Database=privatesocial_dev;User=myuser;Password=mypassword;");
        }
        
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}