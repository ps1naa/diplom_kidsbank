namespace KidBank.Domain.Entities;

public class EducationModule
{
    public Guid Id { get; internal set; }
    public string Title { get; internal set; } = null!;
    public string? Description { get; internal set; }
    public string Content { get; internal set; } = null!;
    public string? ImageUrl { get; internal set; }
    public int OrderIndex { get; internal set; }
    public int XpReward { get; internal set; }
    public int MinAge { get; internal set; }
    public int MaxAge { get; internal set; }
    public bool IsPublished { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public ICollection<Quiz> Quizzes { get; internal set; } = new List<Quiz>();
    public ICollection<EducationProgress> Progresses { get; internal set; } = new List<EducationProgress>();
}
