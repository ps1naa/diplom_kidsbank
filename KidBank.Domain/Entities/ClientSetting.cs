namespace KidBank.Domain.Entities;

public class ClientSetting
{
    public Guid UserId { get; internal set; }
    public string Key { get; internal set; } = string.Empty;
    public string Value { get; internal set; } = string.Empty;
    public DateTime UpdatedAt { get; internal set; }

    public User User { get; internal set; } = null!;
}
