using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.SourceAccountId)
            .HasColumnName("source_account_id");

        builder.Property(t => t.DestinationAccountId)
            .HasColumnName("destination_account_id");

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(t => t.ReferenceId)
            .HasColumnName("reference_id")
            .HasMaxLength(100);

        builder.Property(t => t.RelatedEntityId)
            .HasColumnName("related_entity_id");

        builder.Property(t => t.RelatedEntityType)
            .HasColumnName("related_entity_type")
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(t => t.SourceAccount)
            .WithMany(a => a.SourceTransactions)
            .HasForeignKey(t => t.SourceAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.DestinationAccount)
            .WithMany(a => a.DestinationTransactions)
            .HasForeignKey(t => t.DestinationAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.SourceAccountId)
            .HasDatabaseName("ix_transactions_source_account_id");

        builder.HasIndex(t => t.DestinationAccountId)
            .HasDatabaseName("ix_transactions_destination_account_id");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_transactions_created_at");

        builder.HasIndex(t => new { t.SourceAccountId, t.CreatedAt })
            .HasDatabaseName("ix_transactions_source_date");

        builder.HasIndex(t => new { t.DestinationAccountId, t.CreatedAt })
            .HasDatabaseName("ix_transactions_destination_date");
    }
}
