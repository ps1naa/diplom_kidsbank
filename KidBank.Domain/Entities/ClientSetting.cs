namespace KidBank.Domain.Entities;

public class ClientSetting
{
    public Guid UserId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; internal set; } = string.Empty;
    public DateTime UpdatedAt { get; internal set; }

    public User User { get; private set; } = null!;

    private ClientSetting() { }

    public static ClientSetting Create(Guid userId, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        return new ClientSetting
        {
            UserId = userId,
            Key = key,
            Value = value,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
