using KidBank.Domain.Constants;
using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class CardService
{
    public static VirtualCard Create(Guid accountId, string cardHolderName)
    {
        if (string.IsNullOrWhiteSpace(cardHolderName))
            throw new ArgumentException(ValidationMessages.CardHolderNameRequired, nameof(cardHolderName));

        return new VirtualCard
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CardNumber = GenerateCardNumber(),
            CardHolderName = cardHolderName.ToUpperInvariant(),
            ExpiryDate = DateTime.UtcNow.AddYears(3),
            Cvv = GenerateCvv(),
            IsActive = true,
            IsFrozen = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string GenerateCardNumber()
    {
        var random = new Random();
        var prefix = "4000";
        var middle = string.Concat(Enumerable.Range(0, 12).Select(_ => random.Next(0, 10).ToString()));
        return prefix + middle;
    }

    private static string GenerateCvv()
    {
        var random = new Random();
        return string.Concat(Enumerable.Range(0, 3).Select(_ => random.Next(0, 10).ToString()));
    }

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
