using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class NotificationService
{
    public static void MarkAsRead(Notification notification)
    {
        if (notification.IsRead) return;
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
    }
}
