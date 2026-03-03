using KidBank.Domain.Enums;
using KidBank.Domain.Exceptions;

namespace KidBank.Domain.Entities;

public class TaskAssignment
{
    public Guid Id { get; private set; }
    public Guid AssignedToId { get; private set; }
    public Guid CreatedById { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal RewardAmount { get; private set; }
    public string Currency { get; private set; } = null!;
    public DateTime? DueDate { get; private set; }
    public TaskAssignmentStatus Status { get; private set; }
    public string? ProofUrl { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    public User AssignedTo { get; private set; } = null!;
    public User CreatedBy { get; private set; } = null!;

    private TaskAssignment() { }

    public static TaskAssignment Create(
        Guid assignedToId,
        Guid createdById,
        string title,
        decimal rewardAmount,
        string currency = "RUB",
        string? description = null,
        DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty", nameof(title));

        if (rewardAmount < 0)
            throw new ArgumentException("Reward amount cannot be negative", nameof(rewardAmount));

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

    public void Update(string title, string? description, decimal rewardAmount, DateTime? dueDate)
    {
        if (Status != TaskAssignmentStatus.Pending)
            throw new InvalidOperationDomainException("Can only update pending tasks");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty", nameof(title));

        if (rewardAmount < 0)
            throw new ArgumentException("Reward amount cannot be negative", nameof(rewardAmount));

        Title = title;
        Description = description;
        RewardAmount = rewardAmount;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string? proofUrl = null)
    {
        if (Status != TaskAssignmentStatus.Pending)
            throw new InvalidOperationDomainException("Can only complete pending tasks");

        Status = TaskAssignmentStatus.Completed;
        ProofUrl = proofUrl;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != TaskAssignmentStatus.Completed)
            throw new InvalidOperationDomainException("Can only approve completed tasks");

        Status = TaskAssignmentStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string? reason = null)
    {
        if (Status != TaskAssignmentStatus.Completed)
            throw new InvalidOperationDomainException("Can only reject completed tasks");

        Status = TaskAssignmentStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetToPending()
    {
        if (Status != TaskAssignmentStatus.Rejected)
            throw new InvalidOperationDomainException("Can only reset rejected tasks");

        Status = TaskAssignmentStatus.Pending;
        ProofUrl = null;
        RejectionReason = null;
        CompletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsPending() => Status == TaskAssignmentStatus.Pending;
    public bool IsCompleted() => Status == TaskAssignmentStatus.Completed;
    public bool IsApproved() => Status == TaskAssignmentStatus.Approved;
    public bool IsRejected() => Status == TaskAssignmentStatus.Rejected;
    public bool IsOverdue() => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && IsPending();
}
