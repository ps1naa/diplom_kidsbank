namespace KidBank.Domain.Entities;

public class AchievementDefinition
{
    public Guid Id { get; internal set; }
    public string Code { get; internal set; } = null!;
    public string Title { get; internal set; } = null!;
    public string Description { get; internal set; } = null!;
    public string? IconUrl { get; internal set; }
    public string Category { get; internal set; } = null!;
    public int XpReward { get; internal set; }
    public string? RequirementJson { get; internal set; }
    public bool IsActive { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public ICollection<AchievementProgress> Progresses { get; internal set; } = new List<AchievementProgress>();
}
