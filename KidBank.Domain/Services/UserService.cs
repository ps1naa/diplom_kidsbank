using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class UserService
{
    public static void UpdateProfile(User user, string firstName, string lastName, string? avatarUrl)
    {
        user.FirstName = firstName;
        user.LastName = lastName;
        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;
    }

    public static void AddXp(User user, int xp)
    {
        if (xp <= 0) return;
        user.TotalXp += xp;
        user.UpdatedAt = DateTime.UtcNow;
    }

    public static void UpdateStreak(User user)
    {
        var today = DateTime.UtcNow.Date;
        if (user.LastActivityDate?.Date == today)
            return;
        if (user.LastActivityDate?.Date == today.AddDays(-1))
            user.CurrentStreak++;
        else
            user.CurrentStreak = 1;
        user.LastActivityDate = today;
        user.UpdatedAt = DateTime.UtcNow;
    }
}
