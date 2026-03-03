using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Leaderboard.Queries;

public record GetFamilyLeaderboardQuery : IRequest<Result<FamilyLeaderboardDto>>;

public record FamilyLeaderboardDto(
    Guid FamilyId,
    string FamilyName,
    List<LeaderboardEntryDto> Entries);

public record LeaderboardEntryDto(
    int Rank,
    Guid UserId,
    string Name,
    string? AvatarUrl,
    int TotalXp,
    int Level,
    int CurrentStreak,
    int TasksCompletedThisMonth,
    int GoalsCompleted,
    int AchievementsUnlocked);

public class GetFamilyLeaderboardQueryHandler : IRequestHandler<GetFamilyLeaderboardQuery, Result<FamilyLeaderboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly GamificationService _gamificationService;

    public GetFamilyLeaderboardQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        GamificationService gamificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _gamificationService = gamificationService;
    }

    public async Task<Result<FamilyLeaderboardDto>> Handle(GetFamilyLeaderboardQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.FamilyId.HasValue)
            return Error.InvalidOperation("User does not belong to a family");

        var family = await _context.Families
            .Include(f => f.Members.Where(m => m.Role == UserRole.Kid && !m.IsDeleted))
            .FirstOrDefaultAsync(f => f.Id == _currentUserService.FamilyId.Value, cancellationToken);

        if (family == null)
            return Error.NotFound("Family", _currentUserService.FamilyId.Value);

        var kids = family.Members.ToList();
        var kidIds = kids.Select(k => k.Id).ToList();

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var taskStats = await _context.TaskAssignments
            .Where(t => kidIds.Contains(t.AssignedToId) && 
                        t.Status == TaskAssignmentStatus.Approved &&
                        t.ApprovedAt >= monthStart)
            .GroupBy(t => t.AssignedToId)
            .Select(g => new { KidId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.KidId, x => x.Count, cancellationToken);

        var goalStats = await _context.WishlistGoals
            .Where(g => kidIds.Contains(g.UserId) && g.Status == GoalStatus.Completed)
            .GroupBy(g => g.UserId)
            .Select(g => new { KidId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.KidId, x => x.Count, cancellationToken);

        var achievementStats = await _context.AchievementProgresses
            .Where(ap => kidIds.Contains(ap.UserId) && ap.IsUnlocked)
            .GroupBy(ap => ap.UserId)
            .Select(g => new { KidId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.KidId, x => x.Count, cancellationToken);

        var entries = kids
            .OrderByDescending(k => k.TotalXp)
            .Select((k, index) => new LeaderboardEntryDto(
                index + 1,
                k.Id,
                $"{k.FirstName} {k.LastName}",
                k.AvatarUrl,
                k.TotalXp,
                _gamificationService.CalculateLevel(k.TotalXp),
                k.CurrentStreak,
                taskStats.GetValueOrDefault(k.Id, 0),
                goalStats.GetValueOrDefault(k.Id, 0),
                achievementStats.GetValueOrDefault(k.Id, 0)))
            .ToList();

        return new FamilyLeaderboardDto(family.Id, family.Name, entries);
    }
}
