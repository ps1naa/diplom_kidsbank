namespace KidBank.Domain.Entities;

public class EducationProgress
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ModuleId { get; private set; }
    public bool IsCompleted { get; private set; }
    public int QuizzesCompleted { get; private set; }
    public int QuizzesTotal { get; private set; }
    public int TotalXpEarned { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public User User { get; private set; } = null!;
    public EducationModule Module { get; private set; } = null!;

    private EducationProgress() { }

    public static EducationProgress Create(Guid userId, Guid moduleId, int quizzesTotal)
    {
        return new EducationProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ModuleId = moduleId,
            IsCompleted = false,
            QuizzesCompleted = 0,
            QuizzesTotal = quizzesTotal,
            TotalXpEarned = 0,
            StartedAt = DateTime.UtcNow
        };
    }

    public void RecordQuizCompletion(int xpEarned)
    {
        if (xpEarned < 0)
            throw new ArgumentException("XP earned cannot be negative", nameof(xpEarned));

        QuizzesCompleted++;
        TotalXpEarned += xpEarned;

        if (QuizzesCompleted >= QuizzesTotal)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public decimal GetProgressPercentage()
    {
        if (QuizzesTotal == 0) return 0;
        return Math.Round((decimal)QuizzesCompleted / QuizzesTotal * 100, 2);
    }
}
