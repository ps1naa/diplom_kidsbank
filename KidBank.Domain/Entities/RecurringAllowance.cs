using KidBank.Domain.Services;

namespace KidBank.Domain.Entities;

public class RecurringAllowance
{
    public Guid Id { get; private set; }
    public Guid ParentId { get; private set; }
    public Guid KidId { get; private set; }
    public decimal Amount { get; internal set; }
    public string Currency { get; private set; } = "RUB";
    public string Frequency { get; internal set; } = null!;
    public int DayOfWeek { get; internal set; }
    public int DayOfMonth { get; internal set; }
    public DateTime? NextPaymentDate { get; internal set; }
    public DateTime? LastPaymentDate { get; internal set; }
    public bool IsActive { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; internal set; }

    public User Parent { get; private set; } = null!;
    public User Kid { get; private set; } = null!;

    private RecurringAllowance() { }

    public static RecurringAllowance Create(
        Guid parentId,
        Guid kidId,
        decimal amount,
        string currency,
        string frequency,
        int dayOfWeek,
        int dayOfMonth)
    {
        var allowance = new RecurringAllowance
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            KidId = kidId,
            Amount = amount,
            Currency = currency,
            Frequency = frequency,
            DayOfWeek = dayOfWeek,
            DayOfMonth = dayOfMonth,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        allowance.NextPaymentDate = AllowanceService.CalculateNextPaymentDate(allowance.Frequency, allowance.DayOfWeek, allowance.DayOfMonth);
        return allowance;
    }
}
