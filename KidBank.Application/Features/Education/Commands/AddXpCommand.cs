using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Commands;

public record AddXpCommand(Guid UserId, int Amount) : IRequest<Result<XpResultDto>>;

public record XpResultDto(
    int TotalXp,
    int Level,
    int XpToNextLevel);

public class AddXpCommandHandler : IRequestHandler<AddXpCommand, Result<XpResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly GamificationService _gamificationService;

    public AddXpCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        GamificationService gamificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _gamificationService = gamificationService;
    }

    public async Task<Result<XpResultDto>> Handle(AddXpCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent && _currentUserService.UserId != request.UserId)
        {
            return Error.Forbidden("Cannot add XP to other users");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User", request.UserId);
        }

        UserService.AddXp(user, request.Amount);
        await _context.SaveChangesAsync(cancellationToken);

        var level = _gamificationService.CalculateLevel(user.TotalXp);
        var xpToNext = _gamificationService.GetXpForNextLevel(user.TotalXp);

        return new XpResultDto(user.TotalXp, level, xpToNext);
    }
}
