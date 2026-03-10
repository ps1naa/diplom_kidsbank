using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; internal set; } = null!;
    public string LastName { get; internal set; } = null!;
    public UserRole Role { get; private set; }
    public Guid FamilyId { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string? AvatarUrl { get; internal set; }
    public int TotalXp { get; internal set; }
    public int CurrentStreak { get; internal set; }
    public DateTime? LastActivityDate { get; internal set; }
    public bool IsDeleted { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; internal set; }

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

}
