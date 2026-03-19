namespace KidBank.Domain.Entities;

public class FamilySubscription
{
    public Guid Id { get; internal set; }
    public Guid FamilyId { get; internal set; }
    public Guid SubscribedByUserId { get; internal set; }
    public string Plan { get; internal set; } = null!; // Monthly, Yearly
    public decimal PriceAmount { get; internal set; }
    public string Currency { get; internal set; } = "BYN";
    public DateTime StartDate { get; internal set; }
    public DateTime EndDate { get; internal set; }
    public bool IsActive { get; internal set; }
    public bool AutoRenew { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? CancelledAt { get; internal set; }

    public Family Family { get; internal set; } = null!;
    public User SubscribedByUser { get; internal set; } = null!;
}
