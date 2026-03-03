using KidBank.Domain.Entities;
using KidBank.Domain.Exceptions;

namespace KidBank.Domain.Services;

public class SpendingValidationService
{
    public void ValidateSpending(IEnumerable<SpendingLimit> limits, decimal amount)
    {
        foreach (var limit in limits.Where(l => l.IsActive))
        {
            if (!limit.CanSpend(amount))
            {
                throw new SpendingLimitExceededException(limit.LimitAmount, limit.SpentAmount + amount);
            }
        }
    }

    public void RecordSpending(IEnumerable<SpendingLimit> limits, decimal amount)
    {
        foreach (var limit in limits.Where(l => l.IsActive))
        {
            limit.RecordSpending(amount);
        }
    }

    public decimal GetMinimumAvailableLimit(IEnumerable<SpendingLimit> limits)
    {
        var activeLimits = limits.Where(l => l.IsActive).ToList();
        
        if (!activeLimits.Any())
            return decimal.MaxValue;

        return activeLimits.Min(l => l.GetRemainingLimit());
    }

    public SpendingLimitSummary GetSpendingSummary(IEnumerable<SpendingLimit> limits)
    {
        var activeLimits = limits.Where(l => l.IsActive).ToList();

        return new SpendingLimitSummary
        {
            DailyLimit = activeLimits.FirstOrDefault(l => l.Period == Enums.SpendingLimitPeriod.Daily),
            WeeklyLimit = activeLimits.FirstOrDefault(l => l.Period == Enums.SpendingLimitPeriod.Weekly),
            MonthlyLimit = activeLimits.FirstOrDefault(l => l.Period == Enums.SpendingLimitPeriod.Monthly),
            MinimumAvailable = GetMinimumAvailableLimit(activeLimits)
        };
    }
}

public class SpendingLimitSummary
{
    public SpendingLimit? DailyLimit { get; set; }
    public SpendingLimit? WeeklyLimit { get; set; }
    public SpendingLimit? MonthlyLimit { get; set; }
    public decimal MinimumAvailable { get; set; }
}
