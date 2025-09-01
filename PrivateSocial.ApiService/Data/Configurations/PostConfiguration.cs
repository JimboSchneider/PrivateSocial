using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrivateSocial.ApiService.Data.Entities;

namespace PrivateSocial.ApiService.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Content)
            .IsRequired()
            .HasMaxLength(5000);
            
        builder.HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.CreatedAt);
        
        // Composite index for user's posts ordered by date (common query pattern)
        builder.HasIndex(p => new { p.UserId, p.CreatedAt })
            .HasDatabaseName("IX_Posts_UserId_CreatedAt");
    }
}