using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class EducationModuleService
{
    public static EducationModule Create(
        string title,
        string content,
        int orderIndex,
        int xpReward,
        int minAge = 6,
        int maxAge = 18,
        string? description = null,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Module title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Module content cannot be empty", nameof(content));

        if (xpReward < 0)
            throw new ArgumentException("XP reward cannot be negative", nameof(xpReward));

        return new EducationModule
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Content = content,
            ImageUrl = imageUrl,
            OrderIndex = orderIndex,
            XpReward = xpReward,
            MinAge = minAge,
            MaxAge = maxAge,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void Update(EducationModule module, string title, string content, string? description, string? imageUrl, int xpReward, int minAge, int maxAge)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Module title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Module content cannot be empty", nameof(content));

        module.Title = title;
        module.Content = content;
        module.Description = description;
        module.ImageUrl = imageUrl;
        module.XpReward = xpReward;
        module.MinAge = minAge;
        module.MaxAge = maxAge;
        module.UpdatedAt = DateTime.UtcNow;
    }

    public static void Publish(EducationModule module)
    {
        module.IsPublished = true;
        module.UpdatedAt = DateTime.UtcNow;
    }

    public static void Unpublish(EducationModule module)
    {
        module.IsPublished = false;
        module.UpdatedAt = DateTime.UtcNow;
    }

    public static void SetOrder(EducationModule module, int orderIndex)
    {
        module.OrderIndex = orderIndex;
        module.UpdatedAt = DateTime.UtcNow;
    }

    public static bool IsAvailableForAge(EducationModule module, int age)
    {
        return age >= module.MinAge && age <= module.MaxAge && module.IsPublished;
    }
}
