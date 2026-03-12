using System.Security.Cryptography;
using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class RefreshTokenService
{
    public static RefreshToken Create(Guid userId, string jwtId, int expirationDays = 7, string? deviceInfo = null, string? ipAddress = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateToken(),
            JwtId = jwtId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress
        };
    }

    public static void Revoke(RefreshToken token, string? replacedByToken = null)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = replacedByToken;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
