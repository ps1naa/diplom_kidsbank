using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class RecurringAllowanceConfiguration : IEntityTypeConfiguration<RecurringAllowance>
{
    public void Configure(EntityTypeBuilder<RecurringAllowance> builder)
    {
        builder.ToTable("recurring_allowances");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.ParentId)
            .HasColumnName("parent_id")
            .IsRequired();

        builder.Property(r => r.KidId)
            .HasColumnName("kid_id")
            .IsRequired();

        builder.Property(r => r.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(r => r.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("RUB");

        builder.Property(r => r.Frequency)
            .HasColumnName("frequency")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.DayOfWeek)
            .HasColumnName("day_of_week");

        builder.Property(r => r.DayOfMonth)
            .HasColumnName("day_of_month");

        builder.Property(r => r.NextPaymentDate)
            .HasColumnName("next_payment_date");

        builder.Property(r => r.LastPaymentDate)
            .HasColumnName("last_payment_date");

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(r => r.Parent)
            .WithMany()
            .HasForeignKey(r => r.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Kid)
            .WithMany()
            .HasForeignKey(r => r.KidId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.KidId)
            .HasDatabaseName("ix_recurring_allowances_kid_id");

        builder.HasIndex(r => new { r.KidId, r.IsActive })
            .HasDatabaseName("ix_recurring_allowances_kid_active");
    }
}
