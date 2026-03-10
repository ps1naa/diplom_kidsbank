namespace KidBank.Domain.Entities;

public class AppSetting
{
    public string Key { get; private set; } = string.Empty;
    public string Hostname { get; private set; } = string.Empty;
    public string Value { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }

    private AppSetting() { }

    public static AppSetting Create(string key, string value, string hostname, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));
        if (string.IsNullOrWhiteSpace(hostname))
            throw new ArgumentException("Hostname cannot be empty", nameof(hostname));

        return new AppSetting
        {
            Key = key,
            Hostname = hostname,
            Value = value,
            Description = description,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public const string GlobalHostname = "*";
}
