namespace KidBank.Domain.Entities;

public class VirtualCard
{
    public Guid Id { get; internal set; }
    public Guid AccountId { get; internal set; }
    public string CardNumber { get; internal set; } = null!;
    public string CardHolderName { get; internal set; } = null!;
    public DateTime ExpiryDate { get; internal set; }
    public string Cvv { get; internal set; } = null!;
    public bool IsActive { get; internal set; }
    public bool IsFrozen { get; internal set; }
    public decimal? DailyLimit { get; internal set; }
    public decimal? MonthlyLimit { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public Account Account { get; internal set; } = null!;

    public string MaskedCardNumber => $"**** **** **** {CardNumber[^4..]}";
}
