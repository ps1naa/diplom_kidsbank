namespace KidBank.Application.Common.Interfaces;

public interface ISubscriptionService
{
    Task<bool> IsFamilyProAsync(Guid familyId, CancellationToken ct = default);
    Task<SubscriptionLimits> GetLimitsAsync(Guid familyId, CancellationToken ct = default);
}

public record SubscriptionLimits(
    int MaxKids,
    int MaxGoalsPerKid,
    int MaxCardsPerKid,
    int MaxTaskTemplates,
    bool HasSpendingLimits,
    bool HasCategoryBlocks,
    bool HasAdvancedAnalytics,
    bool HasAllEducation,
    int MaxSavingsAccounts)
{
    public static SubscriptionLimits Free => new(2, 3, 1, 5, false, false, false, false, 1);
    public static SubscriptionLimits Pro => new(10, 20, 5, 50, true, true, true, true, 5);
}
