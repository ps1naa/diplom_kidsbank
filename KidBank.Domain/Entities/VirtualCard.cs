using KidBank.Domain.Constants;

namespace KidBank.Domain.Entities;

public class VirtualCard
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string CardNumber { get; private set; } = null!;
    public string CardHolderName { get; private set; } = null!;
    public DateTime ExpiryDate { get; private set; }
    public string Cvv { get; private set; } = null!;
    public bool IsActive { get; internal set; }
    public bool IsFrozen { get; internal set; }
    public decimal? DailyLimit { get; private set; }
    public decimal? MonthlyLimit { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; internal set; }

    public Account Account { get; private set; } = null!;

    public string MaskedCardNumber => $"**** **** **** {CardNumber[^4..]}";

    private VirtualCard() { }

    public static VirtualCard Create(Guid accountId, string cardHolderName)
    {
        if (string.IsNullOrWhiteSpace(cardHolderName))
            throw new ArgumentException(Constants.ValidationMessages.CardHolderNameRequired, nameof(cardHolderName));

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

}
