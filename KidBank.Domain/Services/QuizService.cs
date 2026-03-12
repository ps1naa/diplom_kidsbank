using System.Text.Json;
using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class QuizService
{
    public static Quiz Create(
        Guid moduleId,
        string question,
        List<string> options,
        int correctOptionIndex,
        int xpReward,
        int orderIndex,
        string? explanation = null)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question cannot be empty", nameof(question));

        if (options == null || options.Count < 2)
            throw new ArgumentException("Quiz must have at least 2 options", nameof(options));

        if (correctOptionIndex < 0 || correctOptionIndex >= options.Count)
            throw new ArgumentException("Correct option index is out of range", nameof(correctOptionIndex));

        if (xpReward < 0)
            throw new ArgumentException("XP reward cannot be negative", nameof(xpReward));

        return new Quiz
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            Question = question,
            OptionsJson = JsonSerializer.Serialize(options),
            CorrectOptionIndex = correctOptionIndex,
            Explanation = explanation,
            XpReward = xpReward,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void Update(Quiz quiz, string question, List<string> options, int correctOptionIndex, string? explanation, int xpReward)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question cannot be empty", nameof(question));

        if (options == null || options.Count < 2)
            throw new ArgumentException("Quiz must have at least 2 options", nameof(options));

        if (correctOptionIndex < 0 || correctOptionIndex >= options.Count)
            throw new ArgumentException("Correct option index is out of range", nameof(correctOptionIndex));

        quiz.Question = question;
        quiz.OptionsJson = JsonSerializer.Serialize(options);
        quiz.CorrectOptionIndex = correctOptionIndex;
        quiz.Explanation = explanation;
        quiz.XpReward = xpReward;
        quiz.UpdatedAt = DateTime.UtcNow;
    }

    public static List<string> GetOptions(Quiz quiz)
    {
        return JsonSerializer.Deserialize<List<string>>(quiz.OptionsJson) ?? new List<string>();
    }

    public static bool IsCorrectAnswer(Quiz quiz, int selectedIndex)
    {
        return selectedIndex == quiz.CorrectOptionIndex;
    }
}
