namespace KidBank.Domain.Entities;

public class CategoryBlock
{
    public Guid Id { get; private set; }
    public Guid KidId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid BlockedById { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User Kid { get; private set; } = null!;
    public SpendingCategory Category { get; private set; } = null!;
    public User BlockedBy { get; private set; } = null!;

    private CategoryBlock() { }

    public static CategoryBlock Create(Guid kidId, Guid categoryId, Guid blockedById)
    {
        return new CategoryBlock
        {
            Id = Guid.NewGuid(),
            KidId = kidId,
            CategoryId = categoryId,
            BlockedById = blockedById,
            CreatedAt = DateTime.UtcNow
        };
    }
}
