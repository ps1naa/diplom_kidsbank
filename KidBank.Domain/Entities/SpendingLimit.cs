using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class SpendingLimit
{
    public Guid Id { get; internal set; }
    public Guid KidId { get; internal set; }
    public Guid SetById { get; internal set; }
    public decimal LimitAmount { get; internal set; }
    public decimal SpentAmount { get; internal set; }
    public string Currency { get; internal set; } = null!;
    public SpendingLimitPeriod Period { get; internal set; }
    public DateTime PeriodStartDate { get; internal set; }
    public DateTime PeriodEndDate { get; internal set; }
    public bool IsActive { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }
    public int Version { get; internal set; }

    public User Kid { get; internal set; } = null!;
    public User SetBy { get; internal set; } = null!;
}
