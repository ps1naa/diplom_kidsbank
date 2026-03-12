using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class Transaction
{
    public Guid Id { get; internal set; }
    public Guid? SourceAccountId { get; internal set; }
    public Guid? DestinationAccountId { get; internal set; }
    public TransactionType Type { get; internal set; }
    public decimal Amount { get; internal set; }
    public string Currency { get; internal set; } = null!;
    public string? Description { get; internal set; }
    public string? ReferenceId { get; internal set; }
    public Guid? RelatedEntityId { get; internal set; }
    public string? RelatedEntityType { get; internal set; }
    public DateTime CreatedAt { get; internal set; }

    public Account? SourceAccount { get; internal set; }
    public Account? DestinationAccount { get; internal set; }
}
