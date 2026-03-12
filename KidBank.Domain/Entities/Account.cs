using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class Account
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public string Name { get; internal set; } = null!;
    public AccountType Type { get; internal set; }
    public decimal Balance { get; internal set; }
    public string Currency { get; internal set; } = null!;
    public bool IsActive { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }
    public int Version { get; internal set; }

    public User User { get; internal set; } = null!;
    public ICollection<Transaction> SourceTransactions { get; internal set; } = new List<Transaction>();
    public ICollection<Transaction> DestinationTransactions { get; internal set; } = new List<Transaction>();
    public ICollection<VirtualCard> VirtualCards { get; internal set; } = new List<VirtualCard>();
}
