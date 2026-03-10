using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class AchievementService
{
    public static void Unlock(AchievementProgress progress)
    {
        if (progress.IsUnlocked) return;
        progress.IsUnlocked = true;
        progress.UnlockedAt = DateTime.UtcNow;
        progress.UpdatedAt = DateTime.UtcNow;
    }
}
