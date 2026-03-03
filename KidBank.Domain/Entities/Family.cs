namespace KidBank.Domain.Entities;

public class Family
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<User> Members { get; private set; } = new List<User>();
    public ICollection<FamilyInvitation> Invitations { get; private set; } = new List<FamilyInvitation>();
    public ICollection<ChatMessage> ChatMessages { get; private set; } = new List<ChatMessage>();

    private Family() { }

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

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Family name cannot be empty", nameof(name));

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public FamilyInvitation CreateInvitation(TimeSpan validFor)
    {
        var invitation = FamilyInvitation.Create(Id, validFor);
        Invitations.Add(invitation);
        return invitation;
    }
}
