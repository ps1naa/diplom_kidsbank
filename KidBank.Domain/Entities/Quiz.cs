namespace KidBank.Domain.Entities;

public class Quiz
{
    public Guid Id { get; private set; }
    public Guid ModuleId { get; private set; }
    public string Question { get; private set; } = null!;
    public string OptionsJson { get; private set; } = null!;
    public int CorrectOptionIndex { get; private set; }
    public string? Explanation { get; private set; }
    public int XpReward { get; private set; }
    public int OrderIndex { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public EducationModule Module { get; private set; } = null!;

    private Quiz() { }

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
            OptionsJson = System.Text.Json.JsonSerializer.Serialize(options),
            CorrectOptionIndex = correctOptionIndex,
            Explanation = explanation,
            XpReward = xpReward,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string question, List<string> options, int correctOptionIndex, string? explanation, int xpReward)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question cannot be empty", nameof(question));

        if (options == null || options.Count < 2)
            throw new ArgumentException("Quiz must have at least 2 options", nameof(options));

        if (correctOptionIndex < 0 || correctOptionIndex >= options.Count)
            throw new ArgumentException("Correct option index is out of range", nameof(correctOptionIndex));

        Question = question;
        OptionsJson = System.Text.Json.JsonSerializer.Serialize(options);
        CorrectOptionIndex = correctOptionIndex;
        Explanation = explanation;
        XpReward = xpReward;
        UpdatedAt = DateTime.UtcNow;
    }

    public List<string> GetOptions()
    {
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(OptionsJson) ?? new List<string>();
    }

    public bool IsCorrectAnswer(int selectedIndex)
    {
        return selectedIndex == CorrectOptionIndex;
    }
}
