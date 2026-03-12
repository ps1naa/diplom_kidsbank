namespace KidBank.Domain.Entities;

public class FamilyInvitation
{
    public Guid Id { get; internal set; }
    public Guid FamilyId { get; internal set; }
    public string Token { get; internal set; } = null!;
    public DateTime ExpiresAt { get; internal set; }
    public bool IsUsed { get; internal set; }
    public Guid? UsedByUserId { get; internal set; }
    public DateTime? UsedAt { get; internal set; }
    public DateTime CreatedAt { get; internal set; }

    public Family Family { get; internal set; } = null!;
    public User? UsedByUser { get; internal set; }

    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
}
