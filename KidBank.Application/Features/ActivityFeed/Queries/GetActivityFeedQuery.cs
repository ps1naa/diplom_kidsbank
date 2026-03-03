using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.ActivityFeed.Queries;

public record GetActivityFeedQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedList<ActivityItemDto>>>;

public record ActivityItemDto(
    Guid Id,
    string Type,
    string Title,
    string Description,
    decimal? Amount,
    string? Currency,
    string? IconType,
    DateTime CreatedAt,
    Dictionary<string, object>? Metadata);

public class GetActivityFeedQueryHandler : IRequestHandler<GetActivityFeedQuery, Result<PaginatedList<ActivityItemDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetActivityFeedQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<ActivityItemDto>>> Handle(GetActivityFeedQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return Error.Unauthorized();

        var userId = _currentUserService.UserId.Value;
        var activities = new List<ActivityItemDto>();

        var accountIds = await _context.Accounts
            .Where(a => a.UserId == userId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var transactions = await _context.Transactions
            .Where(t => (t.SourceAccountId.HasValue && accountIds.Contains(t.SourceAccountId.Value)) ||
                        (t.DestinationAccountId.HasValue && accountIds.Contains(t.DestinationAccountId.Value)))
            .OrderByDescending(t => t.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var t in transactions)
        {
            activities.Add(new ActivityItemDto(
                t.Id,
                t.Amount > 0 ? "income" : "expense",
                t.Amount > 0 ? "Получено" : "Списано",
                t.Description ?? "",
                Math.Abs(t.Amount),
                t.Currency,
                t.Amount > 0 ? "arrow-down" : "arrow-up",
                t.CreatedAt,
                null));
        }

        var tasks = await _context.TaskAssignments
            .Where(t => t.AssignedToId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var t in tasks)
        {
            var statusText = t.Status switch
            {
                TaskAssignmentStatus.Pending => "Новое задание",
                TaskAssignmentStatus.Completed => "Задание выполнено",
                TaskAssignmentStatus.Approved => "Награда получена!",
                TaskAssignmentStatus.Rejected => "Задание отклонено",
                _ => "Задание"
            };

            activities.Add(new ActivityItemDto(
                t.Id,
                "task",
                statusText,
                t.Title,
                t.RewardAmount,
                t.Currency,
                "check-circle",
                t.Status == TaskAssignmentStatus.Approved ? t.ApprovedAt ?? t.CreatedAt : t.CreatedAt,
                new Dictionary<string, object> { { "status", t.Status.ToString() } }));
        }

        var goals = await _context.WishlistGoals
            .Where(g => g.UserId == userId && g.Status == GoalStatus.Completed)
            .OrderByDescending(g => g.CompletedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var g in goals)
        {
            activities.Add(new ActivityItemDto(
                g.Id,
                "goal_completed",
                "Цель достигнута!",
                g.Title,
                g.TargetAmount,
                g.Currency,
                "trophy",
                g.CompletedAt ?? g.CreatedAt,
                null));
        }

        var achievements = await _context.AchievementProgresses
            .Include(ap => ap.AchievementDefinition)
            .Where(ap => ap.UserId == userId && ap.IsUnlocked)
            .OrderByDescending(ap => ap.UnlockedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var a in achievements)
        {
            activities.Add(new ActivityItemDto(
                a.Id,
                "achievement",
                "Достижение разблокировано!",
                a.AchievementDefinition.Title,
                a.AchievementDefinition.XpReward,
                "XP",
                "star",
                a.UnlockedAt ?? a.CreatedAt,
                null));
        }

        var sortedActivities = activities
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedList<ActivityItemDto>(
            sortedActivities,
            activities.Count,
            request.PageNumber,
            request.PageSize);
    }
}
