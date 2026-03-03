using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Analytics.Queries;

public record GetMonthlyStatsQuery(Guid KidId, int Months = 6) : IRequest<Result<List<MonthlyStatsDto>>>;

public record MonthlyStatsDto(
    int Year,
    int Month,
    string MonthName,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetChange,
    int TransactionCount);

public class GetMonthlyStatsQueryHandler : IRequestHandler<GetMonthlyStatsQuery, Result<List<MonthlyStatsDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    private static readonly string[] RussianMonths = 
    {
        "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
    };

    public GetMonthlyStatsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MonthlyStatsDto>>> Handle(GetMonthlyStatsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view monthly stats");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("Kid", request.KidId);
        }

        if (kid.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot view stats for kids from other families");
        }

        var startDate = DateTime.UtcNow.AddMonths(-request.Months + 1);
        startDate = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var kidAccountIds = await _context.Accounts
            .Where(a => a.UserId == request.KidId)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var transactions = await _context.Transactions
            .Where(t => t.CreatedAt >= startDate &&
                        (kidAccountIds.Contains(t.SourceAccountId ?? Guid.Empty) ||
                         kidAccountIds.Contains(t.DestinationAccountId ?? Guid.Empty)))
            .ToListAsync(cancellationToken);

        var stats = new List<MonthlyStatsDto>();

        for (var i = 0; i < request.Months; i++)
        {
            var date = DateTime.UtcNow.AddMonths(-i);
            var monthStart = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var monthTransactions = transactions
                .Where(t => t.CreatedAt >= monthStart && t.CreatedAt < monthEnd)
                .ToList();

            var income = monthTransactions
                .Where(t => kidAccountIds.Contains(t.DestinationAccountId ?? Guid.Empty))
                .Sum(t => t.Amount);

            var expenses = monthTransactions
                .Where(t => kidAccountIds.Contains(t.SourceAccountId ?? Guid.Empty))
                .Sum(t => t.Amount);

            stats.Add(new MonthlyStatsDto(
                date.Year,
                date.Month,
                RussianMonths[date.Month - 1],
                income,
                expenses,
                income - expenses,
                monthTransactions.Count));
        }

        stats.Reverse();
        return stats;
    }
}
