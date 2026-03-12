using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class CategoryBlockService
{
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
