namespace KidBank.Domain.Entities;

public class Notification
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public string Type { get; internal set; } = null!;
    public string Title { get; internal set; } = null!;
    public string Message { get; internal set; } = null!;
    public string? ActionUrl { get; internal set; }
    public bool IsRead { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? ReadAt { get; internal set; }

    public User User { get; internal set; } = null!;
}
