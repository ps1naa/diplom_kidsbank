using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Achievements.Queries;

public record GetAchievementsQuery : IRequest<Result<List<AchievementDto>>>;

public record AchievementDto(
    Guid Id,
    string Code,
    string Title,
    string Description,
    string? IconUrl,
    string Category,
    int XpReward,
    bool IsUnlocked,
    DateTime? UnlockedAt,
    int? Progress,
    int? RequiredProgress);

public class GetAchievementsQueryHandler : IRequestHandler<GetAchievementsQuery, Result<List<AchievementDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAchievementsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AchievementDto>>> Handle(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return Error.Unauthorized();

        var achievements = await _context.AchievementDefinitions
            .Where(a => a.IsActive)
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Title)
            .ToListAsync(cancellationToken);

        var userProgress = await _context.AchievementProgresses
            .Where(ap => ap.UserId == _currentUserService.UserId.Value)
            .ToDictionaryAsync(ap => ap.AchievementDefinitionId, cancellationToken);

        var result = achievements.Select(a =>
        {
            userProgress.TryGetValue(a.Id, out var progress);
            return new AchievementDto(
                a.Id,
                a.Code,
                a.Title,
                a.Description,
                a.IconUrl,
                a.Category,
                a.XpReward,
                progress?.IsUnlocked ?? false,
                progress?.UnlockedAt,
                progress?.CurrentProgress,
                progress?.RequiredProgress);
        }).ToList();

        return result;
    }
}

public record GetMyAchievementsQuery : IRequest<Result<List<AchievementDto>>>;

public class GetMyAchievementsQueryHandler : IRequestHandler<GetMyAchievementsQuery, Result<List<AchievementDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAchievementsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AchievementDto>>> Handle(GetMyAchievementsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return Error.Unauthorized();

        var achievements = await _context.AchievementProgresses
            .Include(ap => ap.AchievementDefinition)
            .Where(ap => ap.UserId == _currentUserService.UserId.Value && ap.IsUnlocked)
            .OrderByDescending(ap => ap.UnlockedAt)
            .Select(ap => new AchievementDto(
                ap.AchievementDefinition.Id,
                ap.AchievementDefinition.Code,
                ap.AchievementDefinition.Title,
                ap.AchievementDefinition.Description,
                ap.AchievementDefinition.IconUrl,
                ap.AchievementDefinition.Category,
                ap.AchievementDefinition.XpReward,
                true,
                ap.UnlockedAt,
                ap.CurrentProgress,
                ap.RequiredProgress))
            .ToListAsync(cancellationToken);

        return achievements;
    }
}
