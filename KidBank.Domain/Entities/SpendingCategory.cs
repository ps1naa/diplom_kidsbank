namespace KidBank.Domain.Entities;

public class SpendingCategory
{
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? IconName { get; private set; }
    public string? ColorHex { get; private set; }
    public bool IsAllowedForKids { get; private set; }
    public bool IsSystem { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Family Family { get; private set; } = null!;

    private SpendingCategory() { }

    public static SpendingCategory Create(
        Guid familyId,
        string name,
        bool isAllowedForKids = true,
        string? iconName = null,
        string? colorHex = null,
        bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        return new SpendingCategory
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            Name = name,
            IconName = iconName,
            ColorHex = colorHex,
            IsAllowedForKids = isAllowedForKids,
            IsSystem = isSystem,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static SpendingCategory CreateSystem(
        Guid familyId,
        string name,
        string iconName,
        string colorHex)
    {
        return Create(familyId, name, true, iconName, colorHex, true);
    }

    public void Update(string name, bool isAllowedForKids, string? iconName, string? colorHex)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot modify system categories");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        Name = name;
        IsAllowedForKids = isAllowedForKids;
        IconName = iconName;
        ColorHex = colorHex;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAllowedForKids(bool isAllowed)
    {
        IsAllowedForKids = isAllowed;
        UpdatedAt = DateTime.UtcNow;
    }
}
