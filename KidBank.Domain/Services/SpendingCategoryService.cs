using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class SpendingCategoryService
{
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

    public static SpendingCategory CreateSystem(Guid familyId, string name, string iconName, string colorHex)
    {
        return Create(familyId, name, true, iconName, colorHex, true);
    }

    public static void Update(SpendingCategory category, string name, bool isAllowedForKids, string? iconName, string? colorHex)
    {
        if (category.IsSystem)
            throw new InvalidOperationException("Cannot modify system categories");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));

        category.Name = name;
        category.IsAllowedForKids = isAllowedForKids;
        category.IconName = iconName;
        category.ColorHex = colorHex;
        category.UpdatedAt = DateTime.UtcNow;
    }

    public static void SetAllowedForKids(SpendingCategory category, bool isAllowed)
    {
        category.IsAllowedForKids = isAllowed;
        category.UpdatedAt = DateTime.UtcNow;
    }
}
