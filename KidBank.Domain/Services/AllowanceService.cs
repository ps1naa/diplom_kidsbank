using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class AllowanceService
{
    public static void Update(RecurringAllowance allowance, decimal amount, string frequency, int dayOfWeek, int dayOfMonth)
    {
        allowance.Amount = amount;
        allowance.Frequency = frequency;
        allowance.DayOfWeek = dayOfWeek;
        allowance.DayOfMonth = dayOfMonth;
        allowance.UpdatedAt = DateTime.UtcNow;
        allowance.NextPaymentDate = GetNextPaymentDate(allowance.Frequency, allowance.DayOfWeek, allowance.DayOfMonth);
    }

    public static void MarkPaymentMade(RecurringAllowance allowance)
    {
        allowance.LastPaymentDate = DateTime.UtcNow;
        allowance.NextPaymentDate = GetNextPaymentDate(allowance.Frequency, allowance.DayOfWeek, allowance.DayOfMonth);
    }

    private static DateTime GetNextPaymentDate(string frequency, int dayOfWeek, int dayOfMonth)
    {
        var now = DateTime.UtcNow.Date;
        return frequency switch
        {
            "Weekly" => GetNextWeekday(now, (DayOfWeek)dayOfWeek),
            "BiWeekly" => GetNextWeekday(now, (DayOfWeek)dayOfWeek).AddDays(
                GetNextWeekday(now, (DayOfWeek)dayOfWeek) <= now ? 14 : 0),
            "Monthly" => GetNextMonthDay(now, dayOfMonth),
            _ => now.AddDays(7)
        };
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

    private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return daysToAdd == 0 ? start.AddDays(7) : start.AddDays(daysToAdd);
    }

    public static DateTime CalculateNextPaymentDate(string frequency, int dayOfWeek, int dayOfMonth)
        => GetNextPaymentDate(frequency, dayOfWeek, dayOfMonth);
}
