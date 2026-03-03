using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class EducationModuleConfiguration : IEntityTypeConfiguration<EducationModule>
{
    public void Configure(EntityTypeBuilder<EducationModule> builder)
    {
        builder.ToTable("education_modules");

        builder.HasKey(em => em.Id);

        builder.Property(em => em.Id)
            .HasColumnName("id");

        builder.Property(em => em.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(em => em.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(em => em.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(em => em.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500);

        builder.Property(em => em.OrderIndex)
            .HasColumnName("order_index")
            .IsRequired();

        builder.Property(em => em.XpReward)
            .HasColumnName("xp_reward")
            .IsRequired();

        builder.Property(em => em.MinAge)
            .HasColumnName("min_age")
            .HasDefaultValue(6);

        builder.Property(em => em.MaxAge)
            .HasColumnName("max_age")
            .HasDefaultValue(18);

        builder.Property(em => em.IsPublished)
            .HasColumnName("is_published")
            .HasDefaultValue(false);

        builder.Property(em => em.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(em => em.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(em => em.OrderIndex)
            .HasDatabaseName("ix_education_modules_order");

        builder.HasIndex(em => em.IsPublished)
            .HasDatabaseName("ix_education_modules_published");
    }
}

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("quizzes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasColumnName("id");

        builder.Property(q => q.ModuleId)
            .HasColumnName("module_id")
            .IsRequired();

        builder.Property(q => q.Question)
            .HasColumnName("question")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(q => q.OptionsJson)
            .HasColumnName("options_json")
            .IsRequired();

        builder.Property(q => q.CorrectOptionIndex)
            .HasColumnName("correct_option_index")
            .IsRequired();

        builder.Property(q => q.Explanation)
            .HasColumnName("explanation")
            .HasMaxLength(1000);

        builder.Property(q => q.XpReward)
            .HasColumnName("xp_reward")
            .IsRequired();

        builder.Property(q => q.OrderIndex)
            .HasColumnName("order_index")
            .IsRequired();

        builder.Property(q => q.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(q => q.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(q => q.Module)
            .WithMany(m => m.Quizzes)
            .HasForeignKey(q => q.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.ModuleId)
            .HasDatabaseName("ix_quizzes_module_id");
    }
}

public class EducationProgressConfiguration : IEntityTypeConfiguration<EducationProgress>
{
    public void Configure(EntityTypeBuilder<EducationProgress> builder)
    {
        builder.ToTable("education_progress");

        builder.HasKey(ep => ep.Id);

        builder.Property(ep => ep.Id)
            .HasColumnName("id");

        builder.Property(ep => ep.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ep => ep.ModuleId)
            .HasColumnName("module_id")
            .IsRequired();

        builder.Property(ep => ep.IsCompleted)
            .HasColumnName("is_completed")
            .HasDefaultValue(false);

        builder.Property(ep => ep.QuizzesCompleted)
            .HasColumnName("quizzes_completed")
            .HasDefaultValue(0);

        builder.Property(ep => ep.QuizzesTotal)
            .HasColumnName("quizzes_total")
            .IsRequired();

        builder.Property(ep => ep.TotalXpEarned)
            .HasColumnName("total_xp_earned")
            .HasDefaultValue(0);

        builder.Property(ep => ep.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(ep => ep.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasOne(ep => ep.User)
            .WithMany(u => u.EducationProgresses)
            .HasForeignKey(ep => ep.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ep => ep.Module)
            .WithMany(m => m.Progresses)
            .HasForeignKey(ep => ep.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ep => ep.UserId)
            .HasDatabaseName("ix_education_progress_user_id");

        builder.HasIndex(ep => new { ep.UserId, ep.ModuleId })
            .IsUnique()
            .HasDatabaseName("ix_education_progress_user_module");
    }
}
