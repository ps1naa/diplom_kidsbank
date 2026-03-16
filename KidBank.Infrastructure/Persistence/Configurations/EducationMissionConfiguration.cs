using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class EducationMissionConfiguration : IEntityTypeConfiguration<EducationMission>
{
    public void Configure(EntityTypeBuilder<EducationMission> builder)
    {
        builder.ToTable("education_missions");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(m => m.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(m => m.OrderIndex).HasColumnName("order_index").IsRequired();
        builder.Property(m => m.XpReward).HasColumnName("xp_reward").IsRequired();
        builder.Property(m => m.MinAge).HasColumnName("min_age").HasDefaultValue(6);
        builder.Property(m => m.MaxAge).HasColumnName("max_age").HasDefaultValue(18);
        builder.Property(m => m.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => m.OrderIndex).HasDatabaseName("ix_education_missions_order");
    }
}
