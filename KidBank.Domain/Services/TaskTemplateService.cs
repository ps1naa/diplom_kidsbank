using KidBank.Domain.Constants;
using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class TaskTemplateService
{
    public static TaskTemplate Create(Guid parentId, string title, decimal rewardAmount,
        string currency = DefaultValues.DefaultCurrency, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(ValidationMessages.TaskTitleRequired, nameof(title));
        if (rewardAmount < 0)
            throw new ArgumentException(ValidationMessages.RewardAmountNonNegative, nameof(rewardAmount));

        return new TaskTemplate
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            Title = title,
            Description = description,
            RewardAmount = rewardAmount,
            Currency = currency.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void Update(TaskTemplate template, string title, decimal rewardAmount, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(ValidationMessages.TaskTitleRequired, nameof(title));
        if (rewardAmount < 0)
            throw new ArgumentException(ValidationMessages.RewardAmountNonNegative, nameof(rewardAmount));

        template.Title = title;
        template.Description = description;
        template.RewardAmount = rewardAmount;
        template.UpdatedAt = DateTime.UtcNow;
    }
}
