using KidBank.Domain.Constants;
using KidBank.Domain.Entities;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Services;

public static class TransactionService
{
    public static Transaction CreateDeposit(Guid accountId, decimal amount, string currency, string? description = null, string? referenceId = null)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = null,
            DestinationAccountId = accountId,
            Type = TransactionType.Deposit,
            Amount = amount,
            Currency = currency,
            Description = description,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateWithdrawal(Guid accountId, decimal amount, string currency, string? description = null)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = accountId,
            DestinationAccountId = null,
            Type = TransactionType.Withdrawal,
            Amount = amount,
            Currency = currency,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateTransfer(Guid sourceAccountId, Guid destinationAccountId, decimal amount, string currency, string? description = null)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.Transfer,
            Amount = amount,
            Currency = currency,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateTaskReward(Guid sourceAccountId, Guid destinationAccountId, decimal amount, string currency, Guid taskId)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.TaskReward,
            Amount = amount,
            Currency = currency,
            Description = "Task reward",
            RelatedEntityId = taskId,
            RelatedEntityType = "TaskAssignment",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateMoneyRequestApproval(Guid sourceAccountId, Guid destinationAccountId, decimal amount, string currency, Guid requestId)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.MoneyRequestApproval,
            Amount = amount,
            Currency = currency,
            Description = "Money request approval",
            RelatedEntityId = requestId,
            RelatedEntityType = "MoneyRequest",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateGoalDeposit(Guid sourceAccountId, decimal amount, string currency, Guid goalId)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = null,
            Type = TransactionType.GoalDeposit,
            Amount = amount,
            Currency = currency,
            Description = "Goal deposit",
            RelatedEntityId = goalId,
            RelatedEntityType = "WishlistGoal",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateGoalWithdrawal(Guid destinationAccountId, decimal amount, string currency, Guid goalId)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = null,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.GoalWithdrawal,
            Amount = amount,
            Currency = currency,
            Description = "Goal withdrawal",
            RelatedEntityId = goalId,
            RelatedEntityType = "WishlistGoal",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException(ValidationMessages.AmountMustBePositive, nameof(amount));
    }
}
