using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public class GamificationService
{
    private const int XpPerLevel = 1000;
    private const int StreakBonusMultiplier = 10;

    public int CalculateLevel(int totalXp)
    {
        return (totalXp / XpPerLevel) + 1;
    }

    public int GetXpForNextLevel(int totalXp)
    {
        var currentLevel = CalculateLevel(totalXp);
        return (currentLevel * XpPerLevel) - totalXp;
    }

    public int CalculateStreakBonus(int currentStreak)
    {
        return currentStreak * StreakBonusMultiplier;
    }

    public void AwardXpForQuizCompletion(User user, int baseXp, bool isCorrectAnswer)
    {
        if (!isCorrectAnswer) return;

        var streakBonus = CalculateStreakBonus(user.CurrentStreak);
        var totalXp = baseXp + streakBonus;
        
        UserService.AddXp(user, totalXp);
        UserService.UpdateStreak(user);
    }

    public void AwardXpForModuleCompletion(User user, int moduleXpReward)
    {
        UserService.AddXp(user, moduleXpReward);
    }

    public void AwardXpForTaskCompletion(User user, int taskXpReward)
    {
        UserService.AddXp(user, taskXpReward);
    }

    public void AwardXpForGoalCompletion(User user, int baseXp)
    {
        var bonus = (int)(baseXp * 0.5);
        UserService.AddXp(user, baseXp + bonus);
    }

    public bool ShouldUnlockAchievement(AchievementProgress progress)
    {
        return progress.CurrentProgress >= progress.RequiredProgress && !progress.IsUnlocked;
    }

    public void ProcessAchievementUnlock(User user, AchievementProgress progress, AchievementDefinition definition)
    {
        if (!ShouldUnlockAchievement(progress)) return;

        AchievementService.Unlock(progress);
        UserService.AddXp(user, definition.XpReward);
    }
}
