namespace KidBank.Domain.Entities;

public class EducationProgress
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public Guid ModuleId { get; internal set; }
    public bool IsCompleted { get; internal set; }
    public int QuizzesCompleted { get; internal set; }
    public int QuizzesTotal { get; internal set; }
    public int TotalXpEarned { get; internal set; }
    public DateTime StartedAt { get; internal set; }
    public DateTime? CompletedAt { get; internal set; }

    public User User { get; internal set; } = null!;
    public EducationModule Module { get; internal set; } = null!;
}
