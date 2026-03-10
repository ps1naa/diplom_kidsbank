namespace KidBank.Domain.Entities;

public class AchievementProgress
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid AchievementDefinitionId { get; private set; }
    public int CurrentProgress { get; internal set; }
    public int RequiredProgress { get; private set; }
    public bool IsUnlocked { get; internal set; }
    public DateTime? UnlockedAt { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; internal set; }

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

}
