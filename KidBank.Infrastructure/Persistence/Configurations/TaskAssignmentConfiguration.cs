using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class TaskAssignmentConfiguration : IEntityTypeConfiguration<TaskAssignment>
{
    public void Configure(EntityTypeBuilder<TaskAssignment> builder)
    {
        builder.ToTable("task_assignments");

        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.Id)
            .HasColumnName("id");

        builder.Property(ta => ta.AssignedToId)
            .HasColumnName("assigned_to_id")
            .IsRequired();

        builder.Property(ta => ta.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired();

        builder.Property(ta => ta.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ta => ta.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(ta => ta.RewardAmount)
            .HasColumnName("reward_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ta => ta.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("RUB")
            .IsRequired();

        builder.Property(ta => ta.DueDate)
            .HasColumnName("due_date");

        builder.Property(ta => ta.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(ta => ta.ProofUrl)
            .HasColumnName("proof_url")
            .HasMaxLength(500);

        builder.Property(ta => ta.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(500);

        builder.Property(ta => ta.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ta => ta.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(ta => ta.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(ta => ta.ApprovedAt)
            .HasColumnName("approved_at");

        builder.HasOne(ta => ta.AssignedTo)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(ta => ta.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.CreatedBy)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(ta => ta.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ta => ta.AssignedToId)
            .HasDatabaseName("ix_task_assignments_assigned_to_id");

        builder.HasIndex(ta => ta.CreatedById)
            .HasDatabaseName("ix_task_assignments_created_by_id");

        builder.HasIndex(ta => new { ta.AssignedToId, ta.Status })
            .HasDatabaseName("ix_task_assignments_assigned_status");
    }
}
