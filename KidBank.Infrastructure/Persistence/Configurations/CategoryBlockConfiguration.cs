using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class CategoryBlockConfiguration : IEntityTypeConfiguration<CategoryBlock>
{
    public void Configure(EntityTypeBuilder<CategoryBlock> builder)
    {
        builder.ToTable("category_blocks");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.KidId)
            .HasColumnName("kid_id")
            .IsRequired();

        builder.Property(c => c.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(c => c.BlockedById)
            .HasColumnName("blocked_by_id")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(c => c.Kid)
            .WithMany()
            .HasForeignKey(c => c.KidId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Category)
            .WithMany()
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.BlockedBy)
            .WithMany()
            .HasForeignKey(c => c.BlockedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.KidId, c.CategoryId })
            .IsUnique()
            .HasDatabaseName("ix_category_blocks_kid_category");
    }
}
