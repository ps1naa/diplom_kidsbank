using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class FamilySubscriptionConfiguration : IEntityTypeConfiguration<FamilySubscription>
{
    public void Configure(EntityTypeBuilder<FamilySubscription> builder)
    {
        builder.ToTable("family_subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.FamilyId).HasColumnName("family_id").IsRequired();
        builder.Property(s => s.SubscribedByUserId).HasColumnName("subscribed_by_user_id").IsRequired();
        builder.Property(s => s.Plan).HasColumnName("plan").HasMaxLength(50).IsRequired();
        builder.Property(s => s.PriceAmount).HasColumnName("price_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(s => s.Currency).HasColumnName("currency").HasMaxLength(10).HasDefaultValue("BYN");
        builder.Property(s => s.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(s => s.EndDate).HasColumnName("end_date").IsRequired();
        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(false);
        builder.Property(s => s.AutoRenew).HasColumnName("auto_renew").HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");

        builder.HasOne(s => s.Family)
            .WithMany(f => f.Subscriptions)
            .HasForeignKey(s => s.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.SubscribedByUser)
            .WithMany()
            .HasForeignKey(s => s.SubscribedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.FamilyId)
            .HasDatabaseName("ix_family_subscriptions_family_id");

        builder.HasIndex(s => new { s.FamilyId, s.IsActive })
            .HasDatabaseName("ix_family_subscriptions_family_active");
    }
}
