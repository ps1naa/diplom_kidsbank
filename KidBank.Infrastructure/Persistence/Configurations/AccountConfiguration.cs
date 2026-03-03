using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(a => a.Balance)
            .HasColumnName("balance")
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(a => a.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("RUB")
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(a => a.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        builder.HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_accounts_user_id");

        builder.HasIndex(a => new { a.UserId, a.Type })
            .HasDatabaseName("ix_accounts_user_type");
    }
}
