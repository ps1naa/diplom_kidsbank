using KidBank.Domain.Entities;
using KidBank.Domain.Exceptions;

namespace KidBank.Domain.Services;

public class LedgerService
{
    public Transaction Deposit(Account account, decimal amount, string? description = null, string? referenceId = null)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        var transaction = Transaction.CreateDeposit(
            account.Id,
            amount,
            account.Currency,
            description,
            referenceId);

        ApplyCredit(account, amount);

        return transaction;
    }

    public Transaction Withdraw(Account account, decimal amount, string? description = null)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (account.Balance < amount)
            throw DomainException.InsufficientFunds();

        var transaction = Transaction.CreateWithdrawal(
            account.Id,
            amount,
            account.Currency,
            description);

        ApplyDebit(account, amount);

        return transaction;
    }

    public Transaction Transfer(Account sourceAccount, Account destinationAccount, decimal amount, string? description = null)
    {
        if (sourceAccount == null)
            throw new ArgumentNullException(nameof(sourceAccount));

        if (destinationAccount == null)
            throw new ArgumentNullException(nameof(destinationAccount));

        if (sourceAccount.Id == destinationAccount.Id)
            throw DomainException.InvalidOperation("Cannot transfer to the same account");

        if (sourceAccount.Currency != destinationAccount.Currency)
            throw DomainException.InvalidOperation("Cannot transfer between accounts with different currencies");

        if (sourceAccount.Balance < amount)
            throw DomainException.InsufficientFunds();

        var transaction = Transaction.CreateTransfer(
            sourceAccount.Id,
            destinationAccount.Id,
            amount,
            sourceAccount.Currency,
            description);

        ApplyDebit(sourceAccount, amount);
        ApplyCredit(destinationAccount, amount);

        return transaction;
    }

    public Transaction TransferTaskReward(
        Account parentAccount,
        Account kidAccount,
        decimal amount,
        Guid taskId)
    {
        if (parentAccount == null)
            throw new ArgumentNullException(nameof(parentAccount));

        if (kidAccount == null)
            throw new ArgumentNullException(nameof(kidAccount));

        if (parentAccount.Balance < amount)
            throw DomainException.InsufficientFunds();

        var transaction = Transaction.CreateTaskReward(
            parentAccount.Id,
            kidAccount.Id,
            amount,
            parentAccount.Currency,
            taskId);

        ApplyDebit(parentAccount, amount);
        ApplyCredit(kidAccount, amount);

        return transaction;
    }

    public Transaction TransferMoneyRequest(
        Account parentAccount,
        Account kidAccount,
        decimal amount,
        Guid requestId)
    {
        if (parentAccount == null)
            throw new ArgumentNullException(nameof(parentAccount));

        if (kidAccount == null)
            throw new ArgumentNullException(nameof(kidAccount));

        if (parentAccount.Balance < amount)
            throw DomainException.InsufficientFunds();

        var transaction = Transaction.CreateMoneyRequestApproval(
            parentAccount.Id,
            kidAccount.Id,
            amount,
            parentAccount.Currency,
            requestId);

        ApplyDebit(parentAccount, amount);
        ApplyCredit(kidAccount, amount);

        return transaction;
    }

    public Transaction DepositToGoal(Account sourceAccount, WishlistGoal goal, decimal amount)
    {
        if (sourceAccount == null)
            throw new ArgumentNullException(nameof(sourceAccount));

        if (goal == null)
            throw new ArgumentNullException(nameof(goal));

        if (sourceAccount.Balance < amount)
            throw DomainException.InsufficientFunds();

        var transaction = Transaction.CreateGoalDeposit(
            sourceAccount.Id,
            amount,
            sourceAccount.Currency,
            goal.Id);

        ApplyDebit(sourceAccount, amount);
        ApplyGoalDeposit(goal, amount);

        return transaction;
    }

    public Transaction WithdrawFromGoal(WishlistGoal goal, Account destinationAccount, decimal amount)
    {
        if (goal == null)
            throw new ArgumentNullException(nameof(goal));

        if (destinationAccount == null)
            throw new ArgumentNullException(nameof(destinationAccount));

        var transaction = Transaction.CreateGoalWithdrawal(
            destinationAccount.Id,
            amount,
            destinationAccount.Currency,
            goal.Id);

        ApplyGoalWithdraw(goal, amount);
        ApplyCredit(destinationAccount, amount);

        return transaction;
    }

    private static void ApplyCredit(Account account, decimal amount)
    {
        if (amount <= 0)
            throw DomainException.InvalidOperation("Credit amount must be positive");
        account.Balance += amount;
        account.UpdatedAt = DateTime.UtcNow;
    }

    private static void ApplyDebit(Account account, decimal amount)
    {
        if (amount <= 0)
            throw DomainException.InvalidOperation("Debit amount must be positive");
        if (account.Balance < amount)
            throw DomainException.InsufficientFunds();
        account.Balance -= amount;
        account.UpdatedAt = DateTime.UtcNow;
    }

    private static void ApplyGoalDeposit(WishlistGoal goal, decimal amount)
    {
        if (goal.Status != Enums.GoalStatus.Active)
            throw DomainException.InvalidOperation("Cannot deposit to a non-active goal");
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive", nameof(amount));
        goal.CurrentAmount += amount;
        goal.UpdatedAt = DateTime.UtcNow;
        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.Status = Enums.GoalStatus.Completed;
            goal.CompletedAt = DateTime.UtcNow;
        }
    }

    private static void ApplyGoalWithdraw(WishlistGoal goal, decimal amount)
    {
        if (goal.Status != Enums.GoalStatus.Active)
            throw DomainException.InvalidOperation("Cannot withdraw from a non-active goal");
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));
        if (amount > goal.CurrentAmount)
            throw DomainException.InsufficientFunds();
        goal.CurrentAmount -= amount;
        goal.UpdatedAt = DateTime.UtcNow;
    }
}
