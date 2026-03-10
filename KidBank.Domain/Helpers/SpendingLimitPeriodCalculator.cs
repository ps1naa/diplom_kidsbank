using KidBank.Domain.Enums;

namespace KidBank.Domain.Helpers;

public static class SpendingLimitPeriodCalculator
{
    public static (DateTime Start, DateTime End) Calculate(SpendingLimitPeriod period, DateTime referenceDate)
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
}
