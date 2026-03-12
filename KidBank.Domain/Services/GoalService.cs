using KidBank.Domain.Constants;
using KidBank.Domain.Entities;
using KidBank.Domain.Exceptions;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Services;

public static class GoalService
{
    public static WishlistGoal Create(Guid userId, string title, decimal targetAmount, string currency = DefaultValues.DefaultCurrency, string? description = null, string? imageUrl = null, DateTime? targetDate = null)
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

    public static void Update(WishlistGoal goal, string title, string? description, string? imageUrl, decimal targetAmount, DateTime? targetDate)
    {
        if (goal.Status != GoalStatus.Active)
            throw DomainException.InvalidOperation("Cannot update a non-active goal");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(ValidationMessages.GoalTitleRequired, nameof(title));
        if (targetAmount <= 0)
            throw new ArgumentException(ValidationMessages.TargetAmountMustBePositive, nameof(targetAmount));
        if (targetAmount < goal.CurrentAmount)
            throw DomainException.InvalidOperation("Target amount cannot be less than current amount");
        goal.Title = title;
        goal.Description = description;
        goal.ImageUrl = imageUrl;
        goal.TargetAmount = targetAmount;
        goal.TargetDate = targetDate;
        goal.UpdatedAt = DateTime.UtcNow;
    }

    public static void Cancel(WishlistGoal goal)
    {
        if (goal.Status != GoalStatus.Active)
            throw DomainException.InvalidOperation("Only active goals can be cancelled");
        goal.Status = GoalStatus.Cancelled;
        goal.UpdatedAt = DateTime.UtcNow;
    }

    public static decimal GetProgress(WishlistGoal goal)
    {
        if (goal.TargetAmount == 0) return 0;
        return Math.Round(goal.CurrentAmount / goal.TargetAmount * 100, 2);
    }
}
