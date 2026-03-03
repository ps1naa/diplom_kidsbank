using KidBank.Domain.Enums;
using KidBank.Domain.Exceptions;

namespace KidBank.Domain.Entities;

public class SpendingLimit
{
    public Guid Id { get; private set; }
    public Guid KidId { get; private set; }
    public Guid SetById { get; private set; }
    public decimal LimitAmount { get; private set; }
    public decimal SpentAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public SpendingLimitPeriod Period { get; private set; }
    public DateTime PeriodStartDate { get; private set; }
    public DateTime PeriodEndDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public User Kid { get; private set; } = null!;
    public User SetBy { get; private set; } = null!;

    private SpendingLimit() { }

    public static SpendingLimit Create(
        Guid kidId,
        Guid setById,
        decimal limitAmount,
        SpendingLimitPeriod period,
        string currency = "RUB")
    {
        if (limitAmount <= 0)
            throw new ArgumentException("Limit amount must be positive", nameof(limitAmount));

        var (startDate, endDate) = CalculatePeriodDates(period, DateTime.UtcNow);

        return new SpendingLimit
        {
            Id = Guid.NewGuid(),
            KidId = kidId,
            SetById = setById,
            LimitAmount = limitAmount,
            SpentAmount = 0,
            Currency = currency.ToUpperInvariant(),
            Period = period,
            PeriodStartDate = startDate,
            PeriodEndDate = endDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };
    }

    private static (DateTime Start, DateTime End) CalculatePeriodDates(SpendingLimitPeriod period, DateTime referenceDate)
    {
        return period switch
        {
            SpendingLimitPeriod.Daily => (
                referenceDate.Date,
                referenceDate.Date.AddDays(1).AddTicks(-1)),
            
            SpendingLimitPeriod.Weekly => (
                referenceDate.Date.AddDays(-(int)referenceDate.DayOfWeek + (int)DayOfWeek.Monday),
                referenceDate.Date.AddDays(7 - (int)referenceDate.DayOfWeek + (int)DayOfWeek.Monday).AddTicks(-1)),
            
            SpendingLimitPeriod.Monthly => (
                new DateTime(referenceDate.Year, referenceDate.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(referenceDate.Year, referenceDate.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddTicks(-1)),
            
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
    }

    public void UpdateLimit(decimal newLimitAmount)
    {
        if (newLimitAmount <= 0)
            throw new ArgumentException("Limit amount must be positive", nameof(newLimitAmount));

        LimitAmount = newLimitAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSpending(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Spending amount must be positive", nameof(amount));

        RefreshPeriodIfNeeded();

        if (SpentAmount + amount > LimitAmount)
            throw new SpendingLimitExceededException(LimitAmount, SpentAmount + amount);

        SpentAmount += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanSpend(decimal amount)
    {
        RefreshPeriodIfNeeded();
        return SpentAmount + amount <= LimitAmount;
    }

    public decimal GetRemainingLimit()
    {
        RefreshPeriodIfNeeded();
        return Math.Max(0, LimitAmount - SpentAmount);
    }

    private void RefreshPeriodIfNeeded()
    {
        if (DateTime.UtcNow > PeriodEndDate)
        {
            var (startDate, endDate) = CalculatePeriodDates(Period, DateTime.UtcNow);
            PeriodStartDate = startDate;
            PeriodEndDate = endDate;
            SpentAmount = 0;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
