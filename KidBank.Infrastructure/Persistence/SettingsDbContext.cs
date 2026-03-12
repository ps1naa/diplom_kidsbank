using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Infrastructure.Persistence;

public class SettingsDbContext : DbContext
{
    public SettingsDbContext(DbContextOptions<SettingsDbContext> options) 
        : base(options)
    {
    }

    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppSetting>(builder =>
        {
            builder.ToTable("app_settings");

            builder.HasKey(e => new { e.Key, e.Hostname });

            builder.Property(e => e.Key)
                .HasColumnName("key")
                .HasMaxLength(255);

            builder.Property(e => e.Hostname)
                .HasColumnName("hostname")
                .HasMaxLength(255);

            builder.Property(e => e.Value)
                .HasColumnName("value")
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
        });
    }
}
