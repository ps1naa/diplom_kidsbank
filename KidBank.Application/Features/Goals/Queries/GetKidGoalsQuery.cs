using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Goals.Commands;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Goals.Queries;

public record GetKidGoalsQuery(Guid KidId, bool IncludeCompleted = false) : IRequest<Result<List<GoalDto>>>;

public class GetKidGoalsQueryHandler : IRequestHandler<GetKidGoalsQuery, Result<List<GoalDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetKidGoalsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GoalDto>>> Handle(GetKidGoalsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view kid's goals");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("Kid", request.KidId);
        }

        if (kid.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot view goals of kids from other families");
        }

        var query = _context.WishlistGoals
            .Where(g => g.UserId == request.KidId);

        if (!request.IncludeCompleted)
        {
            query = query.Where(g => g.Status == GoalStatus.Active);
        }

        var goals = await query
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GoalDto(
                g.Id,
                g.Title,
                g.Description,
                g.ImageUrl,
                g.TargetAmount,
                g.CurrentAmount,
                g.Currency,
                g.TargetDate,
                g.Status.ToString(),
                g.TargetAmount > 0 ? Math.Round(g.CurrentAmount / g.TargetAmount * 100, 2) : 0,
                g.CreatedAt,
                g.CompletedAt))
            .ToListAsync(cancellationToken);

        return goals;
    }
}
