using KidBank.Domain.Enums;

namespace KidBank.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? FamilyId { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsParent { get; }
    bool IsKid { get; }
}
