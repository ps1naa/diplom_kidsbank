using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Goals.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Goals.Queries;

public record GetMyGoalsQuery(bool IncludeCompleted = false) : IRequest<Result<List<GoalDto>>>;

public class GetMyGoalsQueryHandler : IRequestHandler<GetMyGoalsQuery, Result<List<GoalDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetMyGoalsQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<GoalDto>>> Handle(GetMyGoalsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var query = _context.WishlistGoals
            .Where(g => g.UserId == _currentUserService.UserId.Value);

        if (!request.IncludeCompleted)
        {
            query = query.Where(g => g.Status == Domain.Enums.GoalStatus.Active);
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
