using KidBank.Domain.Entities;
using KidBank.Domain.Exceptions;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Services;

public static class TaskService
{
    public static void Update(TaskAssignment task, string title, string? description, decimal rewardAmount, DateTime? dueDate)
    {
        if (task.Status != TaskAssignmentStatus.Pending)
            throw DomainException.InvalidOperation("Can only update pending tasks");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException(Constants.ValidationMessages.TaskTitleRequired, nameof(title));
        if (rewardAmount < 0)
            throw new ArgumentException(Constants.ValidationMessages.RewardAmountNonNegative, nameof(rewardAmount));
        task.Title = title;
        task.Description = description;
        task.RewardAmount = rewardAmount;
        task.DueDate = dueDate;
        task.UpdatedAt = DateTime.UtcNow;
    }

    public static void MarkAsCompleted(TaskAssignment task, string? proofUrl = null)
    {
        if (task.Status != TaskAssignmentStatus.Pending)
            throw DomainException.InvalidOperation("Can only complete pending tasks");
        task.Status = TaskAssignmentStatus.Completed;
        task.ProofUrl = proofUrl;
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
    }

    public static void Approve(TaskAssignment task)
    {
        if (task.Status != TaskAssignmentStatus.Completed)
            throw DomainException.InvalidOperation("Can only approve completed tasks");
        task.Status = TaskAssignmentStatus.Approved;
        task.ApprovedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
    }

    public static void Reject(TaskAssignment task, string? reason = null)
    {
        if (task.Status != TaskAssignmentStatus.Completed)
            throw DomainException.InvalidOperation("Can only reject completed tasks");
        task.Status = TaskAssignmentStatus.Rejected;
        task.RejectionReason = reason;
        task.UpdatedAt = DateTime.UtcNow;
    }
}
