using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class WishlistGoal
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public string Title { get; internal set; } = null!;
    public string? Description { get; internal set; }
    public string? ImageUrl { get; internal set; }
    public decimal TargetAmount { get; internal set; }
    public decimal CurrentAmount { get; internal set; }
    public string Currency { get; internal set; } = null!;
    public DateTime? TargetDate { get; internal set; }
    public GoalStatus Status { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }
    public DateTime? CompletedAt { get; internal set; }
    public int Version { get; internal set; }

    public User User { get; internal set; } = null!;
}
