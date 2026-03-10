using KidBank.Domain.Entities;
using KidBank.Domain.Enums;

namespace KidBank.Application.Common.Interfaces;

public interface IIdentityService
{
    Guid? UserId { get; }
    Guid? FamilyId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsParent { get; }
    bool IsKid { get; }

    (string AccessToken, string JwtId) GenerateAccessToken(User user);
    bool ValidateAccessToken(string token, out string? jwtId);

    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
