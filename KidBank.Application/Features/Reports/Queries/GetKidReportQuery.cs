using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Reports.Queries;

public record GetKidReportQuery(
    Guid KidId,
    string Period = "monthly") : IRequest<Result<KidReportDto>>;

public record KidReportDto(
    Guid KidId,
    string KidName,
    string Period,
    DateTime FromDate,
    DateTime ToDate,
    BalanceSummaryDto Balance,
    List<CategorySpendingDto> SpendingByCategory,
    GoalsSummaryDto Goals,
    TasksSummaryDto Tasks,
    EducationSummaryDto Education);

public record BalanceSummaryDto(
    decimal CurrentBalance,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetChange);

public record CategorySpendingDto(
    string CategoryName,
    decimal Amount,
    int TransactionCount);

public record GoalsSummaryDto(
    int ActiveGoals,
    int CompletedGoals,
    decimal TotalSaved,
    List<GoalProgressDto> Goals);

public record GoalProgressDto(
    string Title,
    decimal CurrentAmount,
    decimal TargetAmount,
    decimal ProgressPercent);

public record TasksSummaryDto(
    int Assigned,
    int Completed,
    int Approved,
    decimal TotalEarned);

public record EducationSummaryDto(
    int ModulesCompleted,
    int QuizzesCompleted,
    int TotalXpEarned,
    int CurrentStreak);

public class GetKidReportQueryValidator : AbstractValidator<GetKidReportQuery>
{
    public GetKidReportQueryValidator()
    {
        RuleFor(x => x.KidId).NotEmpty();
        RuleFor(x => x.Period).Must(p => p == "weekly" || p == "monthly")
            .WithMessage("Period must be 'weekly' or 'monthly'");
    }
}

public class GetKidReportQueryHandler : IRequestHandler<GetKidReportQuery, Result<KidReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public GetKidReportQueryHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result<KidReportDto>> Handle(GetKidReportQuery request, CancellationToken ct)
    {
        if (!_identity.IsParent) return Error.Forbidden("Only parents can view reports");

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, ct);

        if (kid == null) return Error.NotFound("Kid", request.KidId);
        if (kid.FamilyId != _identity.FamilyId) return Error.Forbidden("Kid does not belong to your family");

        var toDate = DateTime.UtcNow;
        var fromDate = request.Period == "weekly" ? toDate.AddDays(-7) : toDate.AddDays(-30);

        var mainAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserId == request.KidId && a.Type == AccountType.Main, ct);

        var accountId = mainAccount?.Id ?? Guid.Empty;
        var transactions = await _context.Transactions
            .Where(t => (t.SourceAccountId == accountId || t.DestinationAccountId == accountId) && t.CreatedAt >= fromDate)
            .ToListAsync(ct);

        var totalIncome = transactions.Where(t =>
            t.Type == TransactionType.Deposit || t.Type == TransactionType.TaskReward ||
            t.Type == TransactionType.MoneyRequestApproval || t.Type == TransactionType.GoalWithdrawal)
            .Sum(t => t.Amount);

        var totalExpenses = transactions.Where(t =>
            t.Type == TransactionType.Withdrawal || t.Type == TransactionType.Transfer ||
            t.Type == TransactionType.GoalDeposit)
            .Sum(t => t.Amount);

        var balance = new BalanceSummaryDto(
            mainAccount?.Balance ?? 0, totalIncome, totalExpenses, totalIncome - totalExpenses);

        var goals = await _context.WishlistGoals.Where(g => g.UserId == request.KidId).ToListAsync(ct);
        var goalsSummary = new GoalsSummaryDto(
            goals.Count(g => g.Status == GoalStatus.Active),
            goals.Count(g => g.Status == GoalStatus.Completed),
            goals.Where(g => g.Status == GoalStatus.Active).Sum(g => g.CurrentAmount),
            goals.Where(g => g.Status == GoalStatus.Active).Select(g => new GoalProgressDto(
                g.Title, g.CurrentAmount, g.TargetAmount,
                g.TargetAmount > 0 ? Math.Round(g.CurrentAmount / g.TargetAmount * 100, 1) : 0)).ToList());

        var tasks = await _context.TaskAssignments
            .Where(t => t.AssignedToId == request.KidId && t.CreatedAt >= fromDate).ToListAsync(ct);
        var tasksSummary = new TasksSummaryDto(
            tasks.Count,
            tasks.Count(t => t.Status == TaskAssignmentStatus.Completed || t.Status == TaskAssignmentStatus.Approved),
            tasks.Count(t => t.Status == TaskAssignmentStatus.Approved),
            tasks.Where(t => t.Status == TaskAssignmentStatus.Approved).Sum(t => t.RewardAmount));

        var progress = await _context.EducationProgresses
            .Where(p => p.UserId == request.KidId && p.StartedAt >= fromDate).ToListAsync(ct);
        var educationSummary = new EducationSummaryDto(
            progress.Count(p => p.IsCompleted),
            progress.Sum(p => p.QuizzesCompleted),
            progress.Sum(p => p.TotalXpEarned),
            kid.CurrentStreak);

        return new KidReportDto(request.KidId, $"{kid.FirstName} {kid.LastName}",
            request.Period, fromDate, toDate, balance,
            new List<CategorySpendingDto>(), goalsSummary, tasksSummary, educationSummary);
    }
}
