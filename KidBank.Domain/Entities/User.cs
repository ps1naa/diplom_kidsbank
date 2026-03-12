using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class User
{
    public Guid Id { get; internal set; }
    public string Email { get; internal set; } = null!;
    public string NormalizedEmailHash { get; internal set; } = null!;
    public string PasswordHash { get; internal set; } = null!;
    public string FirstName { get; internal set; } = null!;
    public string LastName { get; internal set; } = null!;
    public UserRole Role { get; internal set; }
    public Guid FamilyId { get; internal set; }
    public DateTime DateOfBirth { get; internal set; }
    public string? AvatarUrl { get; internal set; }
    public int TotalXp { get; internal set; }
    public int CurrentStreak { get; internal set; }
    public DateTime? LastActivityDate { get; internal set; }
    public bool IsDeleted { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public Family Family { get; internal set; } = null!;
    public ICollection<Account> Accounts { get; internal set; } = new List<Account>();
    public ICollection<RefreshToken> RefreshTokens { get; internal set; } = new List<RefreshToken>();
    public ICollection<TaskAssignment> AssignedTasks { get; internal set; } = new List<TaskAssignment>();
    public ICollection<TaskAssignment> CreatedTasks { get; internal set; } = new List<TaskAssignment>();
    public ICollection<MoneyRequest> MoneyRequests { get; internal set; } = new List<MoneyRequest>();
    public ICollection<WishlistGoal> WishlistGoals { get; internal set; } = new List<WishlistGoal>();
    public ICollection<AchievementProgress> AchievementProgresses { get; internal set; } = new List<AchievementProgress>();
    public ICollection<EducationProgress> EducationProgresses { get; internal set; } = new List<EducationProgress>();
    public ICollection<ChatMessage> SentMessages { get; internal set; } = new List<ChatMessage>();
    public ICollection<ChatMessage> ReceivedMessages { get; internal set; } = new List<ChatMessage>();
}
