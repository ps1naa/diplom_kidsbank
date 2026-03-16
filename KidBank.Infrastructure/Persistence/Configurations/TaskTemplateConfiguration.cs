using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class TaskTemplateConfiguration : IEntityTypeConfiguration<TaskTemplate>
{
    public void Configure(EntityTypeBuilder<TaskTemplate> builder)
    {
        builder.ToTable("task_templates");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.ParentId).HasColumnName("parent_id").IsRequired();
        builder.Property(t => t.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(t => t.RewardAmount).HasColumnName("reward_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("BYN").IsRequired();
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(t => t.Parent)
            .WithMany()
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.ParentId).HasDatabaseName("ix_task_templates_parent_id");
    }
}
