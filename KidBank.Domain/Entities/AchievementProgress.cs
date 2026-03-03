namespace KidBank.Domain.Entities;

public class AchievementProgress
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid AchievementDefinitionId { get; private set; }
    public int CurrentProgress { get; private set; }
    public int RequiredProgress { get; private set; }
    public bool IsUnlocked { get; private set; }
    public DateTime? UnlockedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public User User { get; private set; } = null!;
    public AchievementDefinition AchievementDefinition { get; private set; } = null!;

    private AchievementProgress() { }

    public static AchievementProgress Create(Guid userId, Guid achievementDefinitionId, int requiredProgress)
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

    public void IncrementProgress(int amount = 1)
    {
        if (IsUnlocked) return;

        if (amount <= 0)
            throw new ArgumentException("Progress amount must be positive", nameof(amount));

        CurrentProgress += amount;
        UpdatedAt = DateTime.UtcNow;

        if (CurrentProgress >= RequiredProgress)
        {
            Unlock();
        }
    }

    public void Unlock()
    {
        if (IsUnlocked) return;

        IsUnlocked = true;
        UnlockedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetProgressPercentage()
    {
        if (RequiredProgress == 0) return 0;
        return Math.Min(100, Math.Round((decimal)CurrentProgress / RequiredProgress * 100, 2));
    }
}
