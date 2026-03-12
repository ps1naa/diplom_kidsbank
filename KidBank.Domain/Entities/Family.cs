namespace KidBank.Domain.Entities;

public class Family
{
    public Guid Id { get; internal set; }
    public string Name { get; internal set; } = null!;
    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    public ICollection<User> Members { get; internal set; } = new List<User>();
    public ICollection<FamilyInvitation> Invitations { get; internal set; } = new List<FamilyInvitation>();
    public ICollection<ChatMessage> ChatMessages { get; internal set; } = new List<ChatMessage>();
}
