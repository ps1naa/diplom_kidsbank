using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class EducationProgressService
{
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

    public static void RecordQuizCompletion(EducationProgress progress, int xpEarned)
    {
        if (xpEarned < 0)
            throw new ArgumentException("XP earned cannot be negative", nameof(xpEarned));

        progress.QuizzesCompleted++;
        progress.TotalXpEarned += xpEarned;

        if (progress.QuizzesCompleted >= progress.QuizzesTotal)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }
    }

    public static decimal GetProgressPercentage(EducationProgress progress)
    {
        if (progress.QuizzesTotal == 0) return 0;
        return Math.Round((decimal)progress.QuizzesCompleted / progress.QuizzesTotal * 100, 2);
    }
}
