using KidBank.Domain.Enums;
using KidBank.Domain.Exceptions;

namespace KidBank.Domain.Entities;

public class MoneyRequest
{
    public Guid Id { get; private set; }
    public Guid KidId { get; private set; }
    public Guid ParentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string? Reason { get; private set; }
    public MoneyRequestStatus Status { get; private set; }
    public string? ResponseNote { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    public User Kid { get; private set; } = null!;
    public User Parent { get; private set; } = null!;

    private MoneyRequest() { }

    public static MoneyRequest Create(
        Guid kidId,
        Guid parentId,
        decimal amount,
        string currency = "RUB",
        string? reason = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Request amount must be positive", nameof(amount));

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

    public void Approve(string? note = null)
    {
        if (Status != MoneyRequestStatus.Pending)
            throw new InvalidOperationDomainException("Only pending requests can be approved");

        Status = MoneyRequestStatus.Approved;
        ResponseNote = note;
        RespondedAt = DateTime.UtcNow;
    }

    public void Reject(string? note = null)
    {
        if (Status != MoneyRequestStatus.Pending)
            throw new InvalidOperationDomainException("Only pending requests can be rejected");

        Status = MoneyRequestStatus.Rejected;
        ResponseNote = note;
        RespondedAt = DateTime.UtcNow;
    }

    public bool IsPending() => Status == MoneyRequestStatus.Pending;
    public bool IsApproved() => Status == MoneyRequestStatus.Approved;
    public bool IsRejected() => Status == MoneyRequestStatus.Rejected;
}
