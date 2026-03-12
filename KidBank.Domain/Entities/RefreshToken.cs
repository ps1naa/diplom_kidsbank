namespace KidBank.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public string Token { get; internal set; } = null!;
    public string JwtId { get; internal set; } = null!;
    public DateTime ExpiresAt { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime? RevokedAt { get; internal set; }
    public string? ReplacedByToken { get; internal set; }
    public string? DeviceInfo { get; internal set; }
    public string? IpAddress { get; internal set; }

    public User User { get; internal set; } = null!;

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
}
