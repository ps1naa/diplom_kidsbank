namespace KidBank.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid SenderId { get; private set; }
    public Guid? RecipientId { get; private set; }
    public string Content { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    public Family Family { get; private set; } = null!;
    public User Sender { get; private set; } = null!;
    public User? Recipient { get; private set; }

    private ChatMessage() { }

    public static ChatMessage Create(
        Guid familyId,
        Guid senderId,
        string content,
        Guid? recipientId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty", nameof(content));

        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            SenderId = senderId,
            RecipientId = recipientId,
            Content = content.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ChatMessage CreateFamilyMessage(Guid familyId, Guid senderId, string content)
    {
        return Create(familyId, senderId, content, null);
    }

    public static ChatMessage CreateDirectMessage(Guid familyId, Guid senderId, Guid recipientId, string content)
    {
        if (senderId == recipientId)
            throw new ArgumentException("Cannot send message to yourself", nameof(recipientId));

        return Create(familyId, senderId, content, recipientId);
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }

    public bool IsDirectMessage => RecipientId.HasValue;
    public bool IsFamilyMessage => !RecipientId.HasValue;
}
