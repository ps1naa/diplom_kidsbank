using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_messages");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Id)
            .HasColumnName("id");

        builder.Property(cm => cm.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(cm => cm.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();

        builder.Property(cm => cm.RecipientId)
            .HasColumnName("recipient_id");

        builder.Property(cm => cm.Content)
            .HasColumnName("content")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(cm => cm.IsRead)
            .HasColumnName("is_read")
            .HasDefaultValue(false);

        builder.Property(cm => cm.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(cm => cm.ReadAt)
            .HasColumnName("read_at");

        builder.HasOne(cm => cm.Family)
            .WithMany(f => f.ChatMessages)
            .HasForeignKey(cm => cm.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.Recipient)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(cm => cm.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cm => cm.FamilyId)
            .HasDatabaseName("ix_chat_messages_family_id");

        builder.HasIndex(cm => cm.SenderId)
            .HasDatabaseName("ix_chat_messages_sender_id");

        builder.HasIndex(cm => new { cm.FamilyId, cm.CreatedAt })
            .HasDatabaseName("ix_chat_messages_family_date");
    }
}
