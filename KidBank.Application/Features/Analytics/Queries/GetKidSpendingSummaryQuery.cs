using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Analytics.Queries;

public record GetKidSpendingSummaryQuery(Guid KidId) : IRequest<Result<KidSpendingSummaryDto>>;

public record KidSpendingSummaryDto(
    Guid KidId,
    string KidName,
    decimal TotalBalance,
    decimal TotalSpentThisMonth,
    decimal TotalEarnedThisMonth,
    int TasksCompletedThisMonth,
    decimal TaskRewardsThisMonth,
    int GoalsCompleted,
    int ActiveGoals,
    decimal GoalsSavings);

public class GetKidSpendingSummaryQueryHandler : IRequestHandler<GetKidSpendingSummaryQuery, Result<KidSpendingSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetKidSpendingSummaryQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<KidSpendingSummaryDto>> Handle(GetKidSpendingSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view kid analytics");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("Kid", request.KidId);
        }

        if (kid.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot view analytics for kids from other families");
        }

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var kidAccountIds = await _context.Accounts
            .Where(a => a.UserId == request.KidId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var totalBalance = await _context.Accounts
            .Where(a => a.UserId == request.KidId && a.IsActive)
            .SumAsync(a => a.Balance, cancellationToken);

        var transactions = await _context.Transactions
            .Where(t => t.CreatedAt >= monthStart &&
                        (kidAccountIds.Contains(t.SourceAccountId ?? Guid.Empty) ||
                         kidAccountIds.Contains(t.DestinationAccountId ?? Guid.Empty)))
            .ToListAsync(cancellationToken);

        var totalSpent = transactions
            .Where(t => kidAccountIds.Contains(t.SourceAccountId ?? Guid.Empty) && t.Type != TransactionType.GoalDeposit)
            .Sum(t => t.Amount);

        var totalEarned = transactions
            .Where(t => kidAccountIds.Contains(t.DestinationAccountId ?? Guid.Empty))
            .Sum(t => t.Amount);

        var taskRewards = transactions
            .Where(t => t.Type == TransactionType.TaskReward && kidAccountIds.Contains(t.DestinationAccountId ?? Guid.Empty))
            .Sum(t => t.Amount);

        var tasksCompleted = await _context.TaskAssignments
            .CountAsync(t => t.AssignedToId == request.KidId && 
                             t.Status == TaskAssignmentStatus.Approved && 
                             t.ApprovedAt >= monthStart, 
                         cancellationToken);

        var activeGoals = await _context.WishlistGoals
            .CountAsync(g => g.UserId == request.KidId && g.Status == GoalStatus.Active, cancellationToken);

        var completedGoals = await _context.WishlistGoals
            .CountAsync(g => g.UserId == request.KidId && g.Status == GoalStatus.Completed, cancellationToken);

        var goalsSavings = await _context.WishlistGoals
            .Where(g => g.UserId == request.KidId && g.Status == GoalStatus.Active)
            .SumAsync(g => g.CurrentAmount, cancellationToken);

        return new KidSpendingSummaryDto(
            kid.Id,
            $"{kid.FirstName} {kid.LastName}",
            totalBalance,
            totalSpent,
            totalEarned,
            tasksCompleted,
            taskRewards,
            completedGoals,
            activeGoals,
            goalsSavings);
    }
}
