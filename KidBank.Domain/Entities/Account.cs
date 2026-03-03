using KidBank.Domain.Enums;
using KidBank.Domain.Exceptions;
using KidBank.Domain.ValueObjects;

namespace KidBank.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public AccountType Type { get; private set; }
    public decimal Balance { get; private set; }
    public string Currency { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public User User { get; private set; } = null!;
    public ICollection<Transaction> SourceTransactions { get; private set; } = new List<Transaction>();
    public ICollection<Transaction> DestinationTransactions { get; private set; } = new List<Transaction>();
    public ICollection<VirtualCard> VirtualCards { get; private set; } = new List<VirtualCard>();

    private Account() { }

    public static Account CreateMain(Guid userId, string currency = "RUB")
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Основной счёт",
            Type = AccountType.Main,
            Balance = 0,
            Currency = currency.ToUpperInvariant(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };
    }

    public static Account CreateSavings(Guid userId, string name, string currency = "RUB")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be empty", nameof(name));

        return new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Type = AccountType.Savings,
            Balance = 0,
            Currency = currency.ToUpperInvariant(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationDomainException("Credit amount must be positive");

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationDomainException("Debit amount must be positive");

        if (Balance < amount)
            throw new InsufficientFundsException();

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasSufficientFunds(decimal amount)
    {
        return Balance >= amount;
    }

    public Money GetBalance()
    {
        return Money.Create(Balance, Currency);
    }

    public void Deactivate()
    {
        if (Balance > 0)
            throw new InvalidOperationDomainException("Cannot deactivate account with positive balance");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Account name cannot be empty", nameof(newName));

        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }
}
