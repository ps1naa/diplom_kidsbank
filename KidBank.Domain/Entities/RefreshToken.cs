using System.Security.Cryptography;

namespace KidBank.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public string JwtId { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? DeviceInfo { get; private set; }
    public string? IpAddress { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken() { }

    public static RefreshToken Create(
        Guid userId,
        string jwtId,
        TimeSpan lifetime,
        string? deviceInfo = null,
        string? ipAddress = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateToken(),
            JwtId = jwtId,
            ExpiresAt = DateTime.UtcNow.Add(lifetime),
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;

    public void Revoke(string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}
