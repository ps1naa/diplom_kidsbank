using KidBank.Domain.Constants;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class SpendingLimit
{
    public Guid Id { get; private set; }
    public Guid KidId { get; private set; }
    public Guid SetById { get; private set; }
    public decimal LimitAmount { get; internal set; }
    public decimal SpentAmount { get; internal set; }
    public string Currency { get; private set; } = null!;
    public SpendingLimitPeriod Period { get; private set; }
    public DateTime PeriodStartDate { get; internal set; }
    public DateTime PeriodEndDate { get; internal set; }
    public bool IsActive { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; internal set; }
    public int Version { get; private set; }

    public User Kid { get; private set; } = null!;
    public User SetBy { get; private set; } = null!;

    private SpendingLimit() { }

    public static SpendingLimit Create(
        Guid kidId,
        Guid setById,
        decimal limitAmount,
        SpendingLimitPeriod period,
        string currency = DefaultValues.DefaultCurrency)
    {
        if (limitAmount <= 0)
            throw new ArgumentException(ValidationMessages.AmountMustBePositive, nameof(limitAmount));

        var (startDate, endDate) = Helpers.SpendingLimitPeriodCalculator.Calculate(period, DateTime.UtcNow);

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
}
