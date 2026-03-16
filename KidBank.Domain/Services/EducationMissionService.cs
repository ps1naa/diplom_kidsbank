using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class EducationMissionService
{
    public static EducationMission Create(string title, string? description, int orderIndex,
        int xpReward, int minAge = 6, int maxAge = 18, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Mission title is required", nameof(title));

        return new EducationMission
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            OrderIndex = orderIndex,
            XpReward = xpReward,
            MinAge = minAge,
            MaxAge = maxAge,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
