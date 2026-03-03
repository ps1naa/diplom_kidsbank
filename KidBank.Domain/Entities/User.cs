using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public Guid FamilyId { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string? AvatarUrl { get; private set; }
    public int TotalXp { get; private set; }
    public int CurrentStreak { get; private set; }
    public DateTime? LastActivityDate { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Family Family { get; private set; } = null!;
    public ICollection<Account> Accounts { get; private set; } = new List<Account>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<TaskAssignment> AssignedTasks { get; private set; } = new List<TaskAssignment>();
    public ICollection<TaskAssignment> CreatedTasks { get; private set; } = new List<TaskAssignment>();
    public ICollection<MoneyRequest> MoneyRequests { get; private set; } = new List<MoneyRequest>();
    public ICollection<WishlistGoal> WishlistGoals { get; private set; } = new List<WishlistGoal>();
    public ICollection<AchievementProgress> AchievementProgresses { get; private set; } = new List<AchievementProgress>();
    public ICollection<EducationProgress> EducationProgresses { get; private set; } = new List<EducationProgress>();
    public ICollection<ChatMessage> SentMessages { get; private set; } = new List<ChatMessage>();
    public ICollection<ChatMessage> ReceivedMessages { get; private set; } = new List<ChatMessage>();

    private User() { }

    public static User CreateParent(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Guid familyId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
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

    public static User CreateKid(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Guid familyId)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
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

    public void UpdateProfile(string firstName, string lastName, string? avatarUrl)
    {
        FirstName = firstName;
        LastName = lastName;
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddXp(int xp)
    {
        if (xp <= 0) return;
        TotalXp += xp;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStreak()
    {
        var today = DateTime.UtcNow.Date;
        
        if (LastActivityDate?.Date == today)
            return;

        if (LastActivityDate?.Date == today.AddDays(-1))
        {
            CurrentStreak++;
        }
        else
        {
            CurrentStreak = 1;
        }

        LastActivityDate = today;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetStreak()
    {
        CurrentStreak = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsParent() => Role == UserRole.Parent;
    public bool IsKid() => Role == UserRole.Kid;
}
