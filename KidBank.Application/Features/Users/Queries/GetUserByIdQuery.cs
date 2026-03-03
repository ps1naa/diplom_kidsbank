using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Users.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view other users' profiles");
        }

        var user = await _context.Users
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User", request.UserId);
        }

        if (user.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot access users from other families");
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
