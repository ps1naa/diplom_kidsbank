using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class AchievementDefinitionConfiguration : IEntityTypeConfiguration<AchievementDefinition>
{
    public void Configure(EntityTypeBuilder<AchievementDefinition> builder)
    {
        builder.ToTable("achievement_definitions");

        builder.HasKey(ad => ad.Id);

        builder.Property(ad => ad.Id)
            .HasColumnName("id");

        builder.Property(ad => ad.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ad => ad.Title)
            .HasColumnName("title")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ad => ad.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ad => ad.IconUrl)
            .HasColumnName("icon_url")
            .HasMaxLength(500);

        builder.Property(ad => ad.Category)
            .HasColumnName("category")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ad => ad.XpReward)
            .HasColumnName("xp_reward")
            .IsRequired();

        builder.Property(ad => ad.RequirementJson)
            .HasColumnName("requirement_json");

        builder.Property(ad => ad.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(ad => ad.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ad => ad.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(ad => ad.Code)
            .IsUnique()
            .HasDatabaseName("ix_achievement_definitions_code");

        builder.HasIndex(ad => ad.Category)
            .HasDatabaseName("ix_achievement_definitions_category");
    }
}

public class AchievementProgressConfiguration : IEntityTypeConfiguration<AchievementProgress>
{
    public void Configure(EntityTypeBuilder<AchievementProgress> builder)
    {
        builder.ToTable("achievement_progress");

        builder.HasKey(ap => ap.Id);

        builder.Property(ap => ap.Id)
            .HasColumnName("id");

        builder.Property(ap => ap.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ap => ap.AchievementDefinitionId)
            .HasColumnName("achievement_definition_id")
            .IsRequired();

        builder.Property(ap => ap.CurrentProgress)
            .HasColumnName("current_progress")
            .HasDefaultValue(0);

        builder.Property(ap => ap.RequiredProgress)
            .HasColumnName("required_progress")
            .IsRequired();

        builder.Property(ap => ap.IsUnlocked)
            .HasColumnName("is_unlocked")
            .HasDefaultValue(false);

        builder.Property(ap => ap.UnlockedAt)
            .HasColumnName("unlocked_at");

        builder.Property(ap => ap.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ap => ap.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(ap => ap.User)
            .WithMany(u => u.AchievementProgresses)
            .HasForeignKey(ap => ap.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ap => ap.AchievementDefinition)
            .WithMany(ad => ad.Progresses)
            .HasForeignKey(ap => ap.AchievementDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ap => ap.UserId)
            .HasDatabaseName("ix_achievement_progress_user_id");

        builder.HasIndex(ap => new { ap.UserId, ap.AchievementDefinitionId })
            .IsUnique()
            .HasDatabaseName("ix_achievement_progress_user_achievement");
    }
}
