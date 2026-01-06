using Microsoft.EntityFrameworkCore;
using PrivateSocial.ApiService.Data;

namespace PrivateSocial.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryContext(TimeProvider? timeProvider = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApplicationDbContext(options, timeProvider ?? TimeProvider.System);
    }
}