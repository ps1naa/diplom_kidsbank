using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Commands;

public record UpdateStreakCommand : IRequest<Result<StreakResultDto>>;

public record StreakResultDto(
    int CurrentStreak,
    DateTime? LastActivityDate,
    int BonusXp);

public class UpdateStreakCommandHandler : IRequestHandler<UpdateStreakCommand, Result<StreakResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public UpdateStreakCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StreakResultDto>> Handle(UpdateStreakCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User", _currentUserService.UserId.Value);
        }

        var previousStreak = user.CurrentStreak;
        UserService.UpdateStreak(user);
        
        var bonusXp = 0;
        if (user.CurrentStreak > previousStreak)
        {
            bonusXp = user.CurrentStreak * 10;
            UserService.AddXp(user, bonusXp);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new StreakResultDto(user.CurrentStreak, user.LastActivityDate, bonusXp);
    }
}
