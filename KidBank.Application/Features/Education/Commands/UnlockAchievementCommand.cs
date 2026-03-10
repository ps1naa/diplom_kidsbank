using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Commands;

public record UnlockAchievementCommand(string AchievementCode) : IRequest<Result<AchievementUnlockResultDto>>;

public record AchievementUnlockResultDto(
    Guid AchievementId,
    string Title,
    string Description,
    int XpAwarded,
    bool WasAlreadyUnlocked);

public class UnlockAchievementCommandHandler : IRequestHandler<UnlockAchievementCommand, Result<AchievementUnlockResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly GamificationService _gamificationService;

    public UnlockAchievementCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        GamificationService gamificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _gamificationService = gamificationService;
    }

    public async Task<Result<AchievementUnlockResultDto>> Handle(UnlockAchievementCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var achievement = await _context.AchievementDefinitions
            .FirstOrDefaultAsync(a => a.Code == request.AchievementCode.ToUpperInvariant() && a.IsActive, cancellationToken);

        if (achievement == null)
        {
            return Error.NotFound($"Achievement with code {request.AchievementCode} not found");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User", _currentUserService.UserId.Value);
        }

        var progress = await _context.AchievementProgresses
            .FirstOrDefaultAsync(ap => 
                ap.UserId == _currentUserService.UserId.Value && 
                ap.AchievementDefinitionId == achievement.Id, 
                cancellationToken);

        if (progress == null)
        {
            progress = AchievementProgress.Create(
                _currentUserService.UserId.Value,
                achievement.Id,
                1);
            _context.AchievementProgresses.Add(progress);
        }

        if (progress.IsUnlocked)
        {
            return new AchievementUnlockResultDto(
                achievement.Id,
                achievement.Title,
                achievement.Description,
                0,
                true);
        }

        _gamificationService.ProcessAchievementUnlock(user, progress, achievement);
        await _context.SaveChangesAsync(cancellationToken);

        return new AchievementUnlockResultDto(
            achievement.Id,
            achievement.Title,
            achievement.Description,
            achievement.XpReward,
            false);
    }
}
