using KidBank.Domain.Constants;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public AccountType Type { get; private set; }
    public decimal Balance { get; internal set; }
    public string Currency { get; private set; } = null!;
    public bool IsActive { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; internal set; }
    public int Version { get; private set; }

    public User User { get; private set; } = null!;
    public ICollection<Transaction> SourceTransactions { get; private set; } = new List<Transaction>();
    public ICollection<Transaction> DestinationTransactions { get; private set; } = new List<Transaction>();
    public ICollection<VirtualCard> VirtualCards { get; private set; } = new List<VirtualCard>();

    private Account() { }

    public static Account CreateMain(Guid userId, string currency = DefaultValues.DefaultCurrency)
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = DefaultValues.MainAccountName,
            Type = AccountType.Main,
            Balance = 0,
            Currency = currency.ToUpperInvariant(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };
    }

    public static Account CreateSavings(Guid userId, string name, string currency = DefaultValues.DefaultCurrency)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ValidationMessages.AccountNameRequired, nameof(name));

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

}
