using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class MoneyRequest
{
    public Guid Id { get; internal set; }
    public Guid KidId { get; internal set; }
    public Guid ParentId { get; internal set; }
    public decimal Amount { get; internal set; }
    public string Currency { get; internal set; } = null!;
    public string? Reason { get; internal set; }
    public MoneyRequestStatus Status { get; internal set; }
    public string? ResponseNote { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? RespondedAt { get; internal set; }

    public User Kid { get; internal set; } = null!;
    public User Parent { get; internal set; } = null!;
}
