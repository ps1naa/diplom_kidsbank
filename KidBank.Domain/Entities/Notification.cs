namespace KidBank.Domain.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string? ActionUrl { get; private set; }
    public bool IsRead { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; internal set; }

    public User User { get; private set; } = null!;

    private Notification() { }

    public static Notification Create(
        Guid userId,
        string type,
        string title,
        string message,
        string? actionUrl = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

}
