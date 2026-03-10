using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Analytics.Queries;

public record GetFamilyAnalyticsQuery : IRequest<Result<FamilyAnalyticsDto>>;

public record FamilyAnalyticsDto(
    int TotalKids,
    decimal TotalKidsBalance,
    decimal TotalGoalsSavings,
    int TotalActiveGoals,
    int TotalCompletedGoals,
    int TotalPendingTasks,
    int TotalCompletedTasksThisMonth,
    decimal TotalRewardsPaidThisMonth,
    int TotalPendingMoneyRequests,
    List<KidAnalyticsSummary> KidsSummary);

public record KidAnalyticsSummary(
    Guid KidId,
    string KidName,
    decimal Balance,
    int CurrentStreak,
    int TotalXp,
    int Level,
    int ActiveGoals,
    int PendingTasks);

public class GetFamilyAnalyticsQueryHandler : IRequestHandler<GetFamilyAnalyticsQuery, Result<FamilyAnalyticsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetFamilyAnalyticsQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FamilyAnalyticsDto>> Handle(GetFamilyAnalyticsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view family analytics");
        }

        if (!_currentUserService.FamilyId.HasValue)
        {
            return Error.InvalidOperation("User does not belong to a family");
        }

        var familyId = _currentUserService.FamilyId.Value;
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var kids = await _context.Users
            .Where(u => u.FamilyId == familyId && u.Role == UserRole.Kid && !u.IsDeleted)
            .Include(u => u.Accounts.Where(a => a.IsActive && a.Type == AccountType.Main))
            .Include(u => u.WishlistGoals)
            .Include(u => u.AssignedTasks)
            .ToListAsync(cancellationToken);

        var kidsSummary = kids.Select(k => new KidAnalyticsSummary(
            k.Id,
            $"{k.FirstName} {k.LastName}",
            k.Accounts.Sum(a => a.Balance),
            k.CurrentStreak,
            k.TotalXp,
            (k.TotalXp / 1000) + 1,
            k.WishlistGoals.Count(g => g.Status == GoalStatus.Active),
            k.AssignedTasks.Count(t => t.Status == TaskAssignmentStatus.Pending)))
            .ToList();

        var totalKidsBalance = kids.Sum(k => k.Accounts.Sum(a => a.Balance));
        var totalGoalsSavings = kids.Sum(k => k.WishlistGoals.Where(g => g.Status == GoalStatus.Active).Sum(g => g.CurrentAmount));
        var totalActiveGoals = kids.Sum(k => k.WishlistGoals.Count(g => g.Status == GoalStatus.Active));
        var totalCompletedGoals = kids.Sum(k => k.WishlistGoals.Count(g => g.Status == GoalStatus.Completed));
        var totalPendingTasks = kids.Sum(k => k.AssignedTasks.Count(t => t.Status == TaskAssignmentStatus.Pending || t.Status == TaskAssignmentStatus.Completed));
        var totalCompletedTasksThisMonth = kids.Sum(k => k.AssignedTasks.Count(t => t.Status == TaskAssignmentStatus.Approved && t.ApprovedAt >= monthStart));

        var kidIds = kids.Select(k => k.Id).ToList();
        var totalRewardsPaid = await _context.Transactions
            .Where(t => t.Type == TransactionType.TaskReward &&
                        t.CreatedAt >= monthStart &&
                        kidIds.Contains(t.DestinationAccount!.UserId))
            .SumAsync(t => t.Amount, cancellationToken);

        var totalPendingMoneyRequests = await _context.MoneyRequests
            .CountAsync(mr => mr.Kid.FamilyId == familyId && mr.Status == MoneyRequestStatus.Pending, cancellationToken);

        return new FamilyAnalyticsDto(
            kids.Count,
            totalKidsBalance,
            totalGoalsSavings,
            totalActiveGoals,
            totalCompletedGoals,
            totalPendingTasks,
            totalCompletedTasksThisMonth,
            totalRewardsPaid,
            totalPendingMoneyRequests,
            kidsSummary);
    }
}
