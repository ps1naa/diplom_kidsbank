using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid? SourceAccountId { get; private set; }
    public Guid? DestinationAccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ReferenceId { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Account? SourceAccount { get; private set; }
    public Account? DestinationAccount { get; private set; }

    private Transaction() { }

    public static Transaction CreateDeposit(
        Guid destinationAccountId,
        decimal amount,
        string currency,
        string? description = null,
        string? referenceId = null)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = null,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.Deposit,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Description = description,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateWithdrawal(
        Guid sourceAccountId,
        decimal amount,
        string currency,
        string? description = null)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = null,
            Type = TransactionType.Withdrawal,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateTransfer(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string currency,
        string? description = null)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.Transfer,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateTaskReward(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string currency,
        Guid taskId)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.TaskReward,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Description = "Награда за выполнение задания",
            RelatedEntityId = taskId,
            RelatedEntityType = nameof(TaskAssignment),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateMoneyRequestApproval(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string currency,
        Guid requestId)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.MoneyRequestApproval,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Description = "Одобрение запроса денег",
            RelatedEntityId = requestId,
            RelatedEntityType = nameof(MoneyRequest),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateGoalDeposit(
        Guid sourceAccountId,
        decimal amount,
        string currency,
        Guid goalId)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = sourceAccountId,
            DestinationAccountId = null,
            Type = TransactionType.GoalDeposit,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Description = "Пополнение цели накопления",
            RelatedEntityId = goalId,
            RelatedEntityType = nameof(WishlistGoal),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Transaction CreateGoalWithdrawal(
        Guid destinationAccountId,
        decimal amount,
        string currency,
        Guid goalId)
    {
        ValidateAmount(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            SourceAccountId = null,
            DestinationAccountId = destinationAccountId,
            Type = TransactionType.GoalWithdrawal,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Description = "Вывод средств из цели накопления",
            RelatedEntityId = goalId,
            RelatedEntityType = nameof(WishlistGoal),
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Transaction amount must be positive", nameof(amount));
    }
}
