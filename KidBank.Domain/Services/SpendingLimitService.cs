using KidBank.Domain.Constants;
using KidBank.Domain.Entities;
using KidBank.Domain.Exceptions;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Services;

public static class SpendingLimitHelper
{
    public static SpendingLimit Create(
        Guid kidId,
        Guid setById,
        decimal limitAmount,
        SpendingLimitPeriod period,
        string currency = Constants.DefaultValues.DefaultCurrency)
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

    public static void RefreshPeriodIfNeeded(SpendingLimit limit)
    {
        if (DateTime.UtcNow <= limit.PeriodEndDate) return;
        var (startDate, endDate) = Helpers.SpendingLimitPeriodCalculator.Calculate(limit.Period, DateTime.UtcNow);
        limit.PeriodStartDate = startDate;
        limit.PeriodEndDate = endDate;
        limit.SpentAmount = 0;
        limit.UpdatedAt = DateTime.UtcNow;
    }

    public static bool CanSpend(SpendingLimit limit, decimal amount)
    {
        RefreshPeriodIfNeeded(limit);
        return limit.SpentAmount + amount <= limit.LimitAmount;
    }

    public static decimal GetRemainingLimit(SpendingLimit limit)
    {
        RefreshPeriodIfNeeded(limit);
        return Math.Max(0, limit.LimitAmount - limit.SpentAmount);
    }

    public static void RecordSpending(SpendingLimit limit, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException(ValidationMessages.AmountMustBePositive, nameof(amount));
        RefreshPeriodIfNeeded(limit);
        if (limit.SpentAmount + amount > limit.LimitAmount)
            throw DomainException.SpendingLimitExceeded(limit.LimitAmount, limit.SpentAmount + amount);
        limit.SpentAmount += amount;
        limit.UpdatedAt = DateTime.UtcNow;
    }

    public static void UpdateLimit(SpendingLimit limit, decimal newLimitAmount)
    {
        if (newLimitAmount <= 0)
            throw new ArgumentException(ValidationMessages.AmountMustBePositive, nameof(newLimitAmount));
        limit.LimitAmount = newLimitAmount;
        limit.UpdatedAt = DateTime.UtcNow;
    }
}
