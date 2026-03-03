using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Users.Queries;

public record GetCurrentUserQuery : IRequest<Result<UserProfileDto>>;

public record UserProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string? AvatarUrl,
    DateTime DateOfBirth,
    int TotalXp,
    int CurrentStreak,
    int Level,
    Guid FamilyId,
    string FamilyName,
    DateTime CreatedAt);

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserProfileDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User", _currentUserService.UserId.Value);
        }

        var level = (user.TotalXp / 1000) + 1;

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.AvatarUrl,
            user.DateOfBirth,
            user.TotalXp,
            user.CurrentStreak,
            level,
            user.FamilyId,
            user.Family.Name,
            user.CreatedAt);
    }
}
