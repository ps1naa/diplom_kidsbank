using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class WishlistGoalConfiguration : IEntityTypeConfiguration<WishlistGoal>
{
    public void Configure(EntityTypeBuilder<WishlistGoal> builder)
    {
        builder.ToTable("wishlist_goals");

        builder.HasKey(wg => wg.Id);

        builder.Property(wg => wg.Id)
            .HasColumnName("id");

        builder.Property(wg => wg.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(wg => wg.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(wg => wg.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(wg => wg.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500);

        builder.Property(wg => wg.TargetAmount)
            .HasColumnName("target_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(wg => wg.CurrentAmount)
            .HasColumnName("current_amount")
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(wg => wg.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("RUB")
            .IsRequired();

        builder.Property(wg => wg.TargetDate)
            .HasColumnName("target_date");

        builder.Property(wg => wg.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(wg => wg.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(wg => wg.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(wg => wg.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(wg => wg.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        builder.HasOne(wg => wg.User)
            .WithMany(u => u.WishlistGoals)
            .HasForeignKey(wg => wg.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(wg => wg.UserId)
            .HasDatabaseName("ix_wishlist_goals_user_id");

        builder.HasIndex(wg => new { wg.UserId, wg.Status })
            .HasDatabaseName("ix_wishlist_goals_user_status");
    }
}
