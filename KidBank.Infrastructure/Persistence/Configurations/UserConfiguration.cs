using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.NormalizedEmailHash)
            .HasColumnName("normalized_email_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(u => u.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(u => u.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(u => u.TotalXp)
            .HasColumnName("total_xp")
            .HasDefaultValue(0);

        builder.Property(u => u.CurrentStreak)
            .HasColumnName("current_streak")
            .HasDefaultValue(0);

        builder.Property(u => u.LastActivityDate)
            .HasColumnName("last_activity_date");

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(u => u.Family)
            .WithMany(f => f.Members)
            .HasForeignKey(u => u.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.NormalizedEmailHash)
            .IsUnique()
            .HasDatabaseName("ix_users_normalized_email_hash");

        builder.HasIndex(u => u.FamilyId)
            .HasDatabaseName("ix_users_family_id");

        builder.HasIndex(u => new { u.FamilyId, u.Role })
            .HasDatabaseName("ix_users_family_role");
    }
}
