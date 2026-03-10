using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class ClientSettingConfiguration : IEntityTypeConfiguration<ClientSetting>
{
    public void Configure(EntityTypeBuilder<ClientSetting> builder)
    {
        builder.ToTable("client_settings");

        builder.HasKey(e => new { e.UserId, e.Key });

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.Key)
            .HasColumnName("key")
            .HasMaxLength(255);

        builder.Property(e => e.Value)
            .HasColumnName("value")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
