using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Entities;
using KidBank.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Infrastructure.Persistence;

public class SettingsDbContext : DbContext
{
    private readonly IDataEncryptor? _encryptor;

    public SettingsDbContext(DbContextOptions<SettingsDbContext> options, IDataEncryptor? encryptor = null) 
        : base(options)
    {
        _encryptor = encryptor;
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

            if (_encryptor != null)
            {
                var converter = new EncryptedStringConverter(_encryptor);
                builder.Property(e => e.Value).HasConversion(converter).HasMaxLength(4000);
            }
        });
    }
}
