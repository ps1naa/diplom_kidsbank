namespace KidBank.Domain.Entities;

public class AchievementDefinition
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? IconUrl { get; private set; }
    public string Category { get; private set; } = null!;
    public int XpReward { get; private set; }
    public string? RequirementJson { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<AchievementProgress> Progresses { get; private set; } = new List<AchievementProgress>();

    private AchievementDefinition() { }

    public static AchievementDefinition Create(
        string code,
        string title,
        string description,
        string category,
        int xpReward,
        string? iconUrl = null,
        string? requirementJson = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Achievement code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Achievement title cannot be empty", nameof(title));

        if (xpReward < 0)
            throw new ArgumentException("XP reward cannot be negative", nameof(xpReward));

        return new AchievementDefinition
        {
            Id = Guid.NewGuid(),
            Code = code.ToUpperInvariant(),
            Title = title,
            Description = description,
            IconUrl = iconUrl,
            Category = category,
            XpReward = xpReward,
            RequirementJson = requirementJson,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string description, string? iconUrl, int xpReward)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Achievement title cannot be empty", nameof(title));

        Title = title;
        Description = description;
        IconUrl = iconUrl;
        XpReward = xpReward;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
