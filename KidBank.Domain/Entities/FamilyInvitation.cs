using System.Security.Cryptography;

namespace KidBank.Domain.Entities;

public class FamilyInvitation
{
    public Guid Id { get; private set; }
    public Guid FamilyId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public Guid? UsedByUserId { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Family Family { get; private set; } = null!;
    public User? UsedByUser { get; private set; }

    private FamilyInvitation() { }

    public static FamilyInvitation Create(Guid familyId, TimeSpan validFor)
    {
        return new FamilyInvitation
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.Add(validFor),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow < ExpiresAt;
    }

    public void MarkAsUsed(Guid userId)
    {
        if (!IsValid())
            throw new InvalidOperationException("Invitation is no longer valid");

        IsUsed = true;
        UsedByUserId = userId;
        UsedAt = DateTime.UtcNow;
    }
}
