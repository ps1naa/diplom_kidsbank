using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class NotificationService
{
    public static Notification Create(Guid userId, string type, string title, string message, string? actionUrl = null)
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

    public static void MarkAsRead(Notification notification)
    {
        if (notification.IsRead) return;
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
    }
}
