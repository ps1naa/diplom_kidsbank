using KidBank.Domain.Entities;

namespace KidBank.Application.Common.Interfaces;

public interface IJwtService
{
    (string AccessToken, string JwtId) GenerateAccessToken(User user);
    bool ValidateAccessToken(string token, out string? jwtId);
}
