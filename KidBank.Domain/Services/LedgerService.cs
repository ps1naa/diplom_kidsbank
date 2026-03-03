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

        account.Credit(amount);

        return transaction;
    }

    public Transaction Withdraw(Account account, decimal amount, string? description = null)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (!account.HasSufficientFunds(amount))
            throw new InsufficientFundsException();

        var transaction = Transaction.CreateWithdrawal(
            account.Id,
            amount,
            account.Currency,
            description);

        account.Debit(amount);

        return transaction;
    }

    public Transaction Transfer(Account sourceAccount, Account destinationAccount, decimal amount, string? description = null)
    {
        if (sourceAccount == null)
            throw new ArgumentNullException(nameof(sourceAccount));

        if (destinationAccount == null)
            throw new ArgumentNullException(nameof(destinationAccount));

        if (sourceAccount.Id == destinationAccount.Id)
            throw new InvalidOperationDomainException("Cannot transfer to the same account");

        if (sourceAccount.Currency != destinationAccount.Currency)
            throw new InvalidOperationDomainException("Cannot transfer between accounts with different currencies");

        if (!sourceAccount.HasSufficientFunds(amount))
            throw new InsufficientFundsException();

        var transaction = Transaction.CreateTransfer(
            sourceAccount.Id,
            destinationAccount.Id,
            amount,
            sourceAccount.Currency,
            description);

        sourceAccount.Debit(amount);
        destinationAccount.Credit(amount);

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

        if (!parentAccount.HasSufficientFunds(amount))
            throw new InsufficientFundsException();

        var transaction = Transaction.CreateTaskReward(
            parentAccount.Id,
            kidAccount.Id,
            amount,
            parentAccount.Currency,
            taskId);

        parentAccount.Debit(amount);
        kidAccount.Credit(amount);

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

        if (!parentAccount.HasSufficientFunds(amount))
            throw new InsufficientFundsException();

        var transaction = Transaction.CreateMoneyRequestApproval(
            parentAccount.Id,
            kidAccount.Id,
            amount,
            parentAccount.Currency,
            requestId);

        parentAccount.Debit(amount);
        kidAccount.Credit(amount);

        return transaction;
    }

    public Transaction DepositToGoal(Account sourceAccount, WishlistGoal goal, decimal amount)
    {
        if (sourceAccount == null)
            throw new ArgumentNullException(nameof(sourceAccount));

        if (goal == null)
            throw new ArgumentNullException(nameof(goal));

        if (!sourceAccount.HasSufficientFunds(amount))
            throw new InsufficientFundsException();

        var transaction = Transaction.CreateGoalDeposit(
            sourceAccount.Id,
            amount,
            sourceAccount.Currency,
            goal.Id);

        sourceAccount.Debit(amount);
        goal.Deposit(amount);

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

        goal.Withdraw(amount);
        destinationAccount.Credit(amount);

        return transaction;
    }
}
