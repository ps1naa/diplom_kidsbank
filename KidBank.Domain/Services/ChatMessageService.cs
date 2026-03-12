using KidBank.Domain.Constants;
using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class ChatMessageService
{
    public static ChatMessage Create(Guid familyId, Guid senderId, string content, Guid? recipientId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException(ValidationMessages.MessageContentRequired, nameof(content));

        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            SenderId = senderId,
            RecipientId = recipientId,
            Content = content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static ChatMessage CreateFamilyMessage(Guid familyId, Guid senderId, string content)
    {
        return Create(familyId, senderId, content, recipientId: null);
    }

    public static ChatMessage CreateDirectMessage(Guid familyId, Guid senderId, Guid recipientId, string content)
    {
        return Create(familyId, senderId, content, recipientId);
    }

    public static void MarkAsRead(ChatMessage message)
    {
        if (message.IsRead) return;
        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
    }
}
