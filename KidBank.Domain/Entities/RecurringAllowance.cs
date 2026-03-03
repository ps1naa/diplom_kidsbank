namespace KidBank.Domain.Entities;

public class RecurringAllowance
{
    public Guid Id { get; private set; }
    public Guid ParentId { get; private set; }
    public Guid KidId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "RUB";
    public string Frequency { get; private set; } = null!;
    public int DayOfWeek { get; private set; }
    public int DayOfMonth { get; private set; }
    public DateTime? NextPaymentDate { get; private set; }
    public DateTime? LastPaymentDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

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
        allowance.CalculateNextPaymentDate();
        return allowance;
    }

    public void Update(decimal amount, string frequency, int dayOfWeek, int dayOfMonth)
    {
        Amount = amount;
        Frequency = frequency;
        DayOfWeek = dayOfWeek;
        DayOfMonth = dayOfMonth;
        UpdatedAt = DateTime.UtcNow;
        CalculateNextPaymentDate();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPaymentMade()
    {
        LastPaymentDate = DateTime.UtcNow;
        CalculateNextPaymentDate();
    }

    private void CalculateNextPaymentDate()
    {
        var now = DateTime.UtcNow.Date;
        
        NextPaymentDate = Frequency switch
        {
            "Weekly" => GetNextWeekday(now, (DayOfWeek)DayOfWeek),
            "BiWeekly" => GetNextWeekday(now, (DayOfWeek)DayOfWeek).AddDays(
                GetNextWeekday(now, (DayOfWeek)DayOfWeek) <= now ? 14 : 0),
            "Monthly" => GetNextMonthDay(now, DayOfMonth),
            _ => now.AddDays(7)
        };
    }

    private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return daysToAdd == 0 ? start.AddDays(7) : start.AddDays(daysToAdd);
    }

    private static DateTime GetNextMonthDay(DateTime start, int day)
    {
        var result = new DateTime(start.Year, start.Month, Math.Min(day, DateTime.DaysInMonth(start.Year, start.Month)));
        if (result <= start)
        {
            result = result.AddMonths(1);
            result = new DateTime(result.Year, result.Month, Math.Min(day, DateTime.DaysInMonth(result.Year, result.Month)));
        }
        return result;
    }
}
