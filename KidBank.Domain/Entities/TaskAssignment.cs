using KidBank.Domain.Enums;

namespace KidBank.Domain.Entities;

public class TaskAssignment
{
    public Guid Id { get; internal set; }
    public Guid AssignedToId { get; internal set; }
    public Guid CreatedById { get; internal set; }
    public string Title { get; internal set; } = null!;
    public string? Description { get; internal set; }
    public decimal RewardAmount { get; internal set; }
    public string Currency { get; internal set; } = null!;
    public DateTime? DueDate { get; internal set; }
    public TaskAssignmentStatus Status { get; internal set; }
    public string? ProofUrl { get; internal set; }
    public string? RejectionReason { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }
    public DateTime? CompletedAt { get; internal set; }
    public DateTime? ApprovedAt { get; internal set; }

    public User AssignedTo { get; internal set; } = null!;
    public User CreatedBy { get; internal set; } = null!;
}
