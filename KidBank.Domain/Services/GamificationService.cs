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
        
        user.AddXp(totalXp);
        user.UpdateStreak();
    }

    public void AwardXpForModuleCompletion(User user, int moduleXpReward)
    {
        user.AddXp(moduleXpReward);
    }

    public void AwardXpForTaskCompletion(User user, int taskXpReward)
    {
        user.AddXp(taskXpReward);
    }

    public void AwardXpForGoalCompletion(User user, int baseXp)
    {
        var bonus = (int)(baseXp * 0.5);
        user.AddXp(baseXp + bonus);
    }

    public bool ShouldUnlockAchievement(AchievementProgress progress)
    {
        return progress.CurrentProgress >= progress.RequiredProgress && !progress.IsUnlocked;
    }

    public void ProcessAchievementUnlock(User user, AchievementProgress progress, AchievementDefinition definition)
    {
        if (!ShouldUnlockAchievement(progress)) return;

        progress.Unlock();
        user.AddXp(definition.XpReward);
    }
}
