using Microsoft.EntityFrameworkCore;
using PrivateSocial.ApiService.Data.Entities;

namespace PrivateSocial.ApiService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<WeatherForecastEntity> WeatherForecasts => Set<WeatherForecastEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is WeatherForecastEntity or User or Post &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                if (entityEntry.Entity is WeatherForecastEntity forecast)
                    forecast.CreatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is User user)
                    user.CreatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is Post post)
                    post.CreatedAt = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                if (entityEntry.Entity is WeatherForecastEntity forecast)
                    forecast.UpdatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is User user)
                    user.UpdatedAt = DateTime.UtcNow;
                else if (entityEntry.Entity is Post post)
                    post.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}