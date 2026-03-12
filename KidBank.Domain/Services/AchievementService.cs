using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class AchievementService
{
    public static AchievementProgress CreateProgress(Guid userId, Guid achievementDefinitionId, int requiredProgress)
    {
        if (requiredProgress <= 0)
            throw new ArgumentException("Required progress must be positive", nameof(requiredProgress));

        return new AchievementProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AchievementDefinitionId = achievementDefinitionId,
            CurrentProgress = 0,
            RequiredProgress = requiredProgress,
            IsUnlocked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void Unlock(AchievementProgress progress)
    {
        if (progress.IsUnlocked) return;
        progress.IsUnlocked = true;
        progress.UnlockedAt = DateTime.UtcNow;
        progress.UpdatedAt = DateTime.UtcNow;
    }
}
