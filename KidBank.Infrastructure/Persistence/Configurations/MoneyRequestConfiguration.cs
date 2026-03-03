using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class MoneyRequestConfiguration : IEntityTypeConfiguration<MoneyRequest>
{
    public void Configure(EntityTypeBuilder<MoneyRequest> builder)
    {
        builder.ToTable("money_requests");

        builder.HasKey(mr => mr.Id);

        builder.Property(mr => mr.Id)
            .HasColumnName("id");

        builder.Property(mr => mr.KidId)
            .HasColumnName("kid_id")
            .IsRequired();

        builder.Property(mr => mr.ParentId)
            .HasColumnName("parent_id")
            .IsRequired();

        builder.Property(mr => mr.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(mr => mr.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .HasDefaultValue("RUB")
            .IsRequired();

        builder.Property(mr => mr.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);

        builder.Property(mr => mr.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(mr => mr.ResponseNote)
            .HasColumnName("response_note")
            .HasMaxLength(500);

        builder.Property(mr => mr.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(mr => mr.RespondedAt)
            .HasColumnName("responded_at");

        builder.HasOne(mr => mr.Kid)
            .WithMany(u => u.MoneyRequests)
            .HasForeignKey(mr => mr.KidId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(mr => mr.Parent)
            .WithMany()
            .HasForeignKey(mr => mr.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(mr => mr.KidId)
            .HasDatabaseName("ix_money_requests_kid_id");

        builder.HasIndex(mr => mr.ParentId)
            .HasDatabaseName("ix_money_requests_parent_id");

        builder.HasIndex(mr => new { mr.ParentId, mr.Status })
            .HasDatabaseName("ix_money_requests_parent_status");
    }
}
