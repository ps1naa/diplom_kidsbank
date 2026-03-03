using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace KidBank.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var userIdClaim = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                              ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public Guid? FamilyId
    {
        get
        {
            var familyIdClaim = User?.FindFirst("family_id")?.Value;
            return Guid.TryParse(familyIdClaim, out var familyId) ? familyId : null;
        }
    }

    public UserRole? Role
    {
        get
        {
            var roleClaim = User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsParent => Role == UserRole.Parent;

    public bool IsKid => Role == UserRole.Kid;
}
