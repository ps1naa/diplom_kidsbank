using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class VirtualCardConfiguration : IEntityTypeConfiguration<VirtualCard>
{
    public void Configure(EntityTypeBuilder<VirtualCard> builder)
    {
        builder.ToTable("virtual_cards");

        builder.HasKey(vc => vc.Id);

        builder.Property(vc => vc.Id)
            .HasColumnName("id");

        builder.Property(vc => vc.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(vc => vc.CardNumber)
            .HasColumnName("card_number")
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(vc => vc.CardHolderName)
            .HasColumnName("card_holder_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(vc => vc.ExpiryDate)
            .HasColumnName("expiry_date")
            .IsRequired();

        builder.Property(vc => vc.Cvv)
            .HasColumnName("cvv")
            .HasMaxLength(4)
            .IsRequired();

        builder.Property(vc => vc.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(vc => vc.IsFrozen)
            .HasColumnName("is_frozen")
            .HasDefaultValue(false);

        builder.Property(vc => vc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(vc => vc.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(vc => vc.Account)
            .WithMany(a => a.VirtualCards)
            .HasForeignKey(vc => vc.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(vc => vc.AccountId)
            .HasDatabaseName("ix_virtual_cards_account_id");
    }
}
