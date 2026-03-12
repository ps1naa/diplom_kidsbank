namespace KidBank.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; internal set; }
    public Guid FamilyId { get; internal set; }
    public Guid SenderId { get; internal set; }
    public Guid? RecipientId { get; internal set; }
    public string Content { get; internal set; } = null!;
    public bool IsRead { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? ReadAt { get; internal set; }

    public Family Family { get; internal set; } = null!;
    public User Sender { get; internal set; } = null!;
    public User? Recipient { get; internal set; }

    public bool IsDirectMessage => RecipientId.HasValue;
    public bool IsFamilyMessage => !RecipientId.HasValue;
}
