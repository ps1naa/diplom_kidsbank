namespace KidBank.Domain.Entities;

public class EducationModule
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Content { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public int OrderIndex { get; private set; }
    public int XpReward { get; private set; }
    public int MinAge { get; private set; }
    public int MaxAge { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<Quiz> Quizzes { get; private set; } = new List<Quiz>();
    public ICollection<EducationProgress> Progresses { get; private set; } = new List<EducationProgress>();

    private EducationModule() { }

    public static EducationModule Create(
        string title,
        string content,
        int orderIndex,
        int xpReward,
        int minAge = 6,
        int maxAge = 18,
        string? description = null,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Module title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Module content cannot be empty", nameof(content));

        if (xpReward < 0)
            throw new ArgumentException("XP reward cannot be negative", nameof(xpReward));

        return new EducationModule
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Content = content,
            ImageUrl = imageUrl,
            OrderIndex = orderIndex,
            XpReward = xpReward,
            MinAge = minAge,
            MaxAge = maxAge,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string content, string? description, string? imageUrl, int xpReward, int minAge, int maxAge)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Module title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Module content cannot be empty", nameof(content));

        Title = title;
        Content = content;
        Description = description;
        ImageUrl = imageUrl;
        XpReward = xpReward;
        MinAge = minAge;
        MaxAge = maxAge;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOrder(int orderIndex)
    {
        OrderIndex = orderIndex;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAvailableForAge(int age)
    {
        return age >= MinAge && age <= MaxAge && IsPublished;
    }
}
