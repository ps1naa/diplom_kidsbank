using KidBank.Domain.Entities;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Services;

public static class UserService
{
    public static User CreateParent(string email, string passwordHash, string firstName, string lastName, DateTime dateOfBirth, Guid familyId, string normalizedEmailHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            NormalizedEmailHash = normalizedEmailHash,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.Parent,
            FamilyId = familyId,
            DateOfBirth = DateTime.SpecifyKind(dateOfBirth, DateTimeKind.Utc),
            TotalXp = 0,
            CurrentStreak = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateKid(string email, string passwordHash, string firstName, string lastName, DateTime dateOfBirth, Guid familyId, string normalizedEmailHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            NormalizedEmailHash = normalizedEmailHash,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.Kid,
            FamilyId = familyId,
            DateOfBirth = DateTime.SpecifyKind(dateOfBirth, DateTimeKind.Utc),
            TotalXp = 0,
            CurrentStreak = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

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
