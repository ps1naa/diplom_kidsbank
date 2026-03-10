using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        builder.Property(e => e.Level)
            .HasColumnName("level")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Message)
            .HasColumnName("message")
            .IsRequired();

        builder.Property(e => e.Exception)
            .HasColumnName("exception");

        builder.Property(e => e.RequestPath)
            .HasColumnName("request_path")
            .HasMaxLength(500);

        builder.Property(e => e.RequestMethod)
            .HasColumnName("request_method")
            .HasMaxLength(10);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.UserEmail)
            .HasColumnName("user_email")
            .HasMaxLength(255);

        builder.Property(e => e.StatusCode)
            .HasColumnName("status_code");

        builder.Property(e => e.ElapsedMs)
            .HasColumnName("elapsed_ms");

        builder.Property(e => e.MachineName)
            .HasColumnName("machine_name")
            .HasMaxLength(100);

        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.Level);
        builder.HasIndex(e => e.UserId);
    }
}
