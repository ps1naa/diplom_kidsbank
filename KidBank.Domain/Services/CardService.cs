using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class CardService
{
    public static void Freeze(VirtualCard card)
    {
        card.IsFrozen = true;
        card.UpdatedAt = DateTime.UtcNow;
    }

    public static void Unfreeze(VirtualCard card)
    {
        card.IsFrozen = false;
        card.UpdatedAt = DateTime.UtcNow;
    }

    public static void Deactivate(VirtualCard card)
    {
        card.IsActive = false;
        card.UpdatedAt = DateTime.UtcNow;
    }
}
