using System.Security.Cryptography;
using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class FamilyInvitationService
{
    public static FamilyInvitation Create(Guid familyId, int expirationHours = 48)
    {
        return new FamilyInvitation
        {
            Id = Guid.NewGuid(),
            FamilyId = familyId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static void MarkAsUsed(FamilyInvitation invitation, Guid usedByUserId)
    {
        invitation.IsUsed = true;
        invitation.UsedByUserId = usedByUserId;
        invitation.UsedAt = DateTime.UtcNow;
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
