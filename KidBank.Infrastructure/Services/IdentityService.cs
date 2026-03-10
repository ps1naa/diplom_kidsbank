using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Entities;
using KidBank.Domain.Enums;
using KidBank.Infrastructure.Identity;

namespace KidBank.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly CurrentUserService _currentUser;
    private readonly JwtService _jwt;
    private readonly PasswordHasher _hasher;

    public IdentityService(CurrentUserService currentUser, JwtService jwt, PasswordHasher hasher)
    {
        _currentUser = currentUser;
        _jwt = jwt;
        _hasher = hasher;
    }

    public Guid? UserId => _currentUser.UserId;
    public Guid? FamilyId => _currentUser.FamilyId;
    public UserRole? Role => _currentUser.Role;
    public bool IsAuthenticated => _currentUser.IsAuthenticated;
    public bool IsParent => _currentUser.IsParent;
    public bool IsKid => _currentUser.IsKid;

    public (string AccessToken, string JwtId) GenerateAccessToken(User user) => _jwt.GenerateAccessToken(user);
    public bool ValidateAccessToken(string token, out string? jwtId) => _jwt.ValidateAccessToken(token, out jwtId);

    public string HashPassword(string password) => _hasher.Hash(password);
    public bool VerifyPassword(string password, string hash) => _hasher.Verify(password, hash);
}
