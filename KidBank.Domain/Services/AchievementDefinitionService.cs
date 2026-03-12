using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class AchievementDefinitionService
{
    public static AchievementDefinition Create(
        string code,
        string title,
        string description,
        string category,
        int xpReward,
        string? iconUrl = null,
        string? requirementJson = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Achievement code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Achievement title cannot be empty", nameof(title));

        if (xpReward < 0)
            throw new ArgumentException("XP reward cannot be negative", nameof(xpReward));

        return new AchievementDefinition
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant(),
            Title = title,
            Description = description,
            IconUrl = iconUrl,
            Category = category,
            XpReward = xpReward,
            RequirementJson = requirementJson,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void Update(AchievementDefinition definition, string title, string description, string? iconUrl, int xpReward)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Achievement title cannot be empty", nameof(title));

        definition.Title = title;
        definition.Description = description;
        definition.IconUrl = iconUrl;
        definition.XpReward = xpReward;
        definition.UpdatedAt = DateTime.UtcNow;
    }

    public static void Activate(AchievementDefinition definition)
    {
        definition.IsActive = true;
        definition.UpdatedAt = DateTime.UtcNow;
    }

    public static void Deactivate(AchievementDefinition definition)
    {
        definition.IsActive = false;
        definition.UpdatedAt = DateTime.UtcNow;
    }
}
