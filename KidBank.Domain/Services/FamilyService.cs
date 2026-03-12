using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class FamilyService
{
    public static Family Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Family name cannot be empty", nameof(name));

        return new Family
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void UpdateName(Family family, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Family name cannot be empty", nameof(name));
        family.Name = name;
        family.UpdatedAt = DateTime.UtcNow;
    }
}
