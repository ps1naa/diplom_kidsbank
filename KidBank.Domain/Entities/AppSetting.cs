namespace KidBank.Domain.Entities;

public class AppSetting
{
    public const string GlobalHostname = "*";

    public string Key { get; internal set; } = string.Empty;
    public string Hostname { get; internal set; } = string.Empty;
    public string Value { get; internal set; } = string.Empty;
    public string? Description { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
