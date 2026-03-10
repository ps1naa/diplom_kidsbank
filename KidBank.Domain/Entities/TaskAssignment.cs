using KidBank.Domain.Constants;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class TaskAssignment
{
    public Guid Id { get; private set; }
    public Guid AssignedToId { get; private set; }
    public Guid CreatedById { get; private set; }
    public string Title { get; internal set; } = null!;
    public string? Description { get; internal set; }
    public decimal RewardAmount { get; internal set; }
    public string Currency { get; private set; } = null!;
    public DateTime? DueDate { get; internal set; }
    public TaskAssignmentStatus Status { get; internal set; }
    public string? ProofUrl { get; internal set; }
    public string? RejectionReason { get; internal set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; internal set; }
    public DateTime? CompletedAt { get; internal set; }
    public DateTime? ApprovedAt { get; internal set; }

    public User AssignedTo { get; private set; } = null!;
    public User CreatedBy { get; private set; } = null!;

    private TaskAssignment() { }

    public static TaskAssignment Create(
        Guid assignedToId,
        Guid createdById,
        string title,
        decimal rewardAmount,
        string currency = DefaultValues.DefaultCurrency,
        string? description = null,
        DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(ValidationMessages.TaskTitleRequired, nameof(title));

        if (rewardAmount < 0)
            throw new ArgumentException(ValidationMessages.RewardAmountNonNegative, nameof(rewardAmount));

        return new TaskAssignment
        {
            Id = Guid.NewGuid(),
            AssignedToId = assignedToId,
            CreatedById = createdById,
            Title = title,
            Description = description,
            RewardAmount = rewardAmount,
            Currency = currency.ToUpperInvariant(),
            DueDate = dueDate,
            Status = TaskAssignmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }
}
