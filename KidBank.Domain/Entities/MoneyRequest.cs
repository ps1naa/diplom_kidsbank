using KidBank.Domain.Constants;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class MoneyRequest
{
    public Guid Id { get; private set; }
    public Guid KidId { get; private set; }
    public Guid ParentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? Reason { get; private set; }
    public MoneyRequestStatus Status { get; internal set; }
    public string? ResponseNote { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RespondedAt { get; internal set; }

    public User Kid { get; private set; } = null!;
    public User Parent { get; private set; } = null!;

    private MoneyRequest() { }

    public static MoneyRequest Create(
        Guid kidId,
        Guid parentId,
        decimal amount,
        string currency = DefaultValues.DefaultCurrency,
        string? reason = null)
    {
        if (amount <= 0)
            throw new ArgumentException(ValidationMessages.AmountMustBePositive, nameof(amount));

        return new MoneyRequest
        {
            Id = Guid.NewGuid(),
            KidId = kidId,
            ParentId = parentId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Reason = reason,
            Status = MoneyRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
}
