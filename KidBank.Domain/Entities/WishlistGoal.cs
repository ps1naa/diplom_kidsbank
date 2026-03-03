using KidBank.Domain.Enums;
using KidBank.Domain.Exceptions;

namespace KidBank.Domain.Entities;

public class WishlistGoal
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public DateTime? TargetDate { get; private set; }
    public GoalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int Version { get; private set; }

    public User User { get; private set; } = null!;

    private WishlistGoal() { }

    public static WishlistGoal Create(
        Guid userId,
        string title,
        decimal targetAmount,
        string currency = "RUB",
        string? description = null,
        string? imageUrl = null,
        DateTime? targetDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Goal title cannot be empty", nameof(title));

        if (targetAmount <= 0)
            throw new ArgumentException("Target amount must be positive", nameof(targetAmount));

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

    public void Update(string title, string? description, string? imageUrl, decimal targetAmount, DateTime? targetDate)
    {
        if (Status != GoalStatus.Active)
            throw new InvalidOperationDomainException("Cannot update a non-active goal");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Goal title cannot be empty", nameof(title));

        if (targetAmount <= 0)
            throw new ArgumentException("Target amount must be positive", nameof(targetAmount));

        if (targetAmount < CurrentAmount)
            throw new InvalidOperationDomainException("Target amount cannot be less than current amount");

        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        TargetAmount = targetAmount;
        TargetDate = targetDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deposit(decimal amount)
    {
        if (Status != GoalStatus.Active)
            throw new InvalidOperationDomainException("Cannot deposit to a non-active goal");

        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive", nameof(amount));

        CurrentAmount += amount;
        UpdatedAt = DateTime.UtcNow;

        if (CurrentAmount >= TargetAmount)
        {
            Status = GoalStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public void Withdraw(decimal amount)
    {
        if (Status != GoalStatus.Active)
            throw new InvalidOperationDomainException("Cannot withdraw from a non-active goal");

        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));

        if (amount > CurrentAmount)
            throw new InsufficientFundsException();

        CurrentAmount -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != GoalStatus.Active)
            throw new InvalidOperationDomainException("Only active goals can be cancelled");

        Status = GoalStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal GetProgress()
    {
        if (TargetAmount == 0) return 0;
        return Math.Round(CurrentAmount / TargetAmount * 100, 2);
    }

    public decimal GetRemainingAmount()
    {
        return Math.Max(0, TargetAmount - CurrentAmount);
    }
}
