using KidBank.Domain.Constants;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class WishlistGoal
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; internal set; } = null!;
    public string? Description { get; internal set; }
    public string? ImageUrl { get; internal set; }
    public decimal TargetAmount { get; internal set; }
    public decimal CurrentAmount { get; internal set; }
    public string Currency { get; private set; } = null!;
    public DateTime? TargetDate { get; internal set; }
    public GoalStatus Status { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; internal set; }
    public DateTime? CompletedAt { get; internal set; }
    public int Version { get; private set; }

    public User User { get; private set; } = null!;

    private WishlistGoal() { }

    public static WishlistGoal Create(
        Guid userId,
        string title,
        decimal targetAmount,
        string currency = DefaultValues.DefaultCurrency,
        string? description = null,
        string? imageUrl = null,
        DateTime? targetDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(ValidationMessages.GoalTitleRequired, nameof(title));

        if (targetAmount <= 0)
            throw new ArgumentException(ValidationMessages.TargetAmountMustBePositive, nameof(targetAmount));

        return new WishlistGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Description = description,
            ImageUrl = imageUrl,
            TargetAmount = targetAmount,
            CurrentAmount = 0,
            Currency = currency.ToUpperInvariant(),
            TargetDate = targetDate,
            Status = GoalStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };
    }

}
