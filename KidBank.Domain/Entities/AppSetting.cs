namespace KidBank.Domain.Entities;

public class AppSetting
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }

    private AppSetting() { }

    public static AppSetting Create(string key, string value, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        return new AppSetting
        {
            Key = key,
            Value = value,
            Description = description,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
