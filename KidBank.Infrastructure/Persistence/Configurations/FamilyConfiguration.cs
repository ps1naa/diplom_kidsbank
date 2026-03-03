using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidBank.Infrastructure.Persistence.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at");
    }
}

public class FamilyInvitationConfiguration : IEntityTypeConfiguration<FamilyInvitation>
{
    public void Configure(EntityTypeBuilder<FamilyInvitation> builder)
    {
        builder.ToTable("family_invitations");

        builder.HasKey(fi => fi.Id);

        builder.Property(fi => fi.Id)
            .HasColumnName("id");

        builder.Property(fi => fi.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(fi => fi.Token)
            .HasColumnName("token")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(fi => fi.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(fi => fi.IsUsed)
            .HasColumnName("is_used")
            .HasDefaultValue(false);

        builder.Property(fi => fi.UsedByUserId)
            .HasColumnName("used_by_user_id");

        builder.Property(fi => fi.UsedAt)
            .HasColumnName("used_at");

        builder.Property(fi => fi.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(fi => fi.Family)
            .WithMany(f => f.Invitations)
            .HasForeignKey(fi => fi.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fi => fi.UsedByUser)
            .WithMany()
            .HasForeignKey(fi => fi.UsedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(fi => fi.Token)
            .IsUnique()
            .HasDatabaseName("ix_family_invitations_token");

        builder.HasIndex(fi => fi.FamilyId)
            .HasDatabaseName("ix_family_invitations_family_id");
    }
}
