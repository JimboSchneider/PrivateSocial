using Microsoft.EntityFrameworkCore;
using PrivateSocial.ApiService.Data.Entities;

namespace PrivateSocial.ApiService.Data;

public class ApplicationDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();

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
            .Where(e => e.Entity is User or Post &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                if (entityEntry.Entity is User user)
                    user.CreatedAt = now;
                else if (entityEntry.Entity is Post post)
                    post.CreatedAt = now;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                if (entityEntry.Entity is User user)
                    user.UpdatedAt = now;
                else if (entityEntry.Entity is Post post)
                    post.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}