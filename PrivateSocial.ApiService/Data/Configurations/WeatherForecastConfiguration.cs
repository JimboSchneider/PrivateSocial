using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrivateSocial.ApiService.Data.Entities;

namespace PrivateSocial.ApiService.Data.Configurations;

public class WeatherForecastConfiguration : IEntityTypeConfiguration<WeatherForecastEntity>
{
    public void Configure(EntityTypeBuilder<WeatherForecastEntity> builder)
    {
        builder.ToTable("WeatherForecasts");
        
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.Summary)
            .HasMaxLength(100);
            
        builder.HasIndex(w => w.Date);
    }
}