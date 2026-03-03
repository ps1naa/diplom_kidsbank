using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class SpendingLimitConfiguration : IEntityTypeConfiguration<SpendingLimit>
{
    public void Configure(EntityTypeBuilder<SpendingLimit> builder)
    {
        builder.ToTable("spending_limits");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Id)
            .HasColumnName("id");

        builder.Property(sl => sl.KidId)
            .HasColumnName("kid_id")
            .IsRequired();

        builder.Property(sl => sl.SetById)
            .HasColumnName("set_by_id")
            .IsRequired();

        builder.Property(sl => sl.LimitAmount)
            .HasColumnName("limit_amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sl => sl.SpentAmount)
            .HasColumnName("spent_amount")
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(sl => sl.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("RUB")
            .IsRequired();

        builder.Property(sl => sl.Period)
            .HasColumnName("period")
            .IsRequired();

        builder.Property(sl => sl.PeriodStartDate)
            .HasColumnName("period_start_date")
            .IsRequired();

        builder.Property(sl => sl.PeriodEndDate)
            .HasColumnName("period_end_date")
            .IsRequired();

        builder.Property(sl => sl.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(sl => sl.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(sl => sl.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(sl => sl.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        builder.HasOne(sl => sl.Kid)
            .WithMany()
            .HasForeignKey(sl => sl.KidId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.SetBy)
            .WithMany()
            .HasForeignKey(sl => sl.SetById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sl => sl.KidId)
            .HasDatabaseName("ix_spending_limits_kid_id");

        builder.HasIndex(sl => new { sl.KidId, sl.Period, sl.IsActive })
            .HasDatabaseName("ix_spending_limits_kid_period_active");
    }
}

public class SpendingCategoryConfiguration : IEntityTypeConfiguration<SpendingCategory>
{
    public void Configure(EntityTypeBuilder<SpendingCategory> builder)
    {
        builder.ToTable("spending_categories");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.Id)
            .HasColumnName("id");

        builder.Property(sc => sc.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(sc => sc.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(sc => sc.IconName)
            .HasColumnName("icon_name")
            .HasMaxLength(50);

        builder.Property(sc => sc.ColorHex)
            .HasColumnName("color_hex")
            .HasMaxLength(7);

        builder.Property(sc => sc.IsAllowedForKids)
            .HasColumnName("is_allowed_for_kids")
            .HasDefaultValue(true);

        builder.Property(sc => sc.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false);

        builder.Property(sc => sc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(sc => sc.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(sc => sc.Family)
            .WithMany()
            .HasForeignKey(sc => sc.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sc => sc.FamilyId)
            .HasDatabaseName("ix_spending_categories_family_id");
    }
}
