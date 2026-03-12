namespace KidBank.Domain.Entities;

public class CategoryBlock
{
    public Guid Id { get; internal set; }
    public Guid KidId { get; internal set; }
    public Guid CategoryId { get; internal set; }
    public Guid BlockedById { get; internal set; }
    public DateTime CreatedAt { get; internal set; }

    public User Kid { get; internal set; } = null!;
    public SpendingCategory Category { get; internal set; } = null!;
    public User BlockedBy { get; internal set; } = null!;
}
