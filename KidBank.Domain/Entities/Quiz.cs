namespace KidBank.Domain.Entities;

public class Quiz
{
    public Guid Id { get; internal set; }
    public Guid ModuleId { get; internal set; }
    public string Question { get; internal set; } = null!;
    public string OptionsJson { get; internal set; } = null!;
    public int CorrectOptionIndex { get; internal set; }
    public string? Explanation { get; internal set; }
    public int XpReward { get; internal set; }
    public int OrderIndex { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public EducationModule Module { get; internal set; } = null!;
}
