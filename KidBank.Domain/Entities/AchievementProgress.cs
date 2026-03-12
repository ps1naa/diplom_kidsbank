namespace KidBank.Domain.Entities;

public class AchievementProgress
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public Guid AchievementDefinitionId { get; internal set; }
    public int CurrentProgress { get; internal set; }
    public int RequiredProgress { get; internal set; }
    public bool IsUnlocked { get; internal set; }
    public DateTime? UnlockedAt { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public User User { get; internal set; } = null!;
    public AchievementDefinition AchievementDefinition { get; internal set; } = null!;
}
