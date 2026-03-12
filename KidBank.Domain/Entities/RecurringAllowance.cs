namespace KidBank.Domain.Entities;

public class RecurringAllowance
{
    public Guid Id { get; internal set; }
    public Guid ParentId { get; internal set; }
    public Guid KidId { get; internal set; }
    public decimal Amount { get; internal set; }
    public string Currency { get; internal set; } = "RUB";
    public string Frequency { get; internal set; } = null!;
    public int DayOfWeek { get; internal set; }
    public int DayOfMonth { get; internal set; }
    public DateTime? NextPaymentDate { get; internal set; }
    public DateTime? LastPaymentDate { get; internal set; }
    public bool IsActive { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public User Parent { get; internal set; } = null!;
    public User Kid { get; internal set; } = null!;
}
