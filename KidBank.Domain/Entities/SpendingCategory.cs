namespace KidBank.Domain.Entities;

public class SpendingCategory
{
    public Guid Id { get; internal set; }
    public Guid FamilyId { get; internal set; }
    public string Name { get; internal set; } = null!;
    public string? IconName { get; internal set; }
    public string? ColorHex { get; internal set; }
    public bool IsAllowedForKids { get; internal set; }
    public bool IsSystem { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public Family Family { get; internal set; } = null!;
}
