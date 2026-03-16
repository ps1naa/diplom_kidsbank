namespace KidBank.Domain.Entities;

public class TaskTemplate
{
    public Guid Id { get; internal set; }
    public Guid ParentId { get; internal set; }
    public string Title { get; internal set; } = null!;
    public string? Description { get; internal set; }
    public decimal RewardAmount { get; internal set; }
    public string Currency { get; internal set; } = null!;
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public User Parent { get; internal set; } = null!;
}
