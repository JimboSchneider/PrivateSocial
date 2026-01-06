using Microsoft.EntityFrameworkCore;
using PrivateSocial.ApiService.Data;

namespace PrivateSocial.Tests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected ApplicationDbContext Context { get; private set; } = null!;
    private readonly string _databaseName;

    protected IntegrationTestBase()
    {
        // Use a unique database name for each test class to avoid conflicts
        _databaseName = $"TestDb_{Guid.NewGuid()}";
    }

    public virtual async ValueTask InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        Context = new ApplicationDbContext(options, TimeProvider.System);
        await Context.Database.EnsureCreatedAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }
}
