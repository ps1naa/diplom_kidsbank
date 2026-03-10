using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Families.Queries;

public record GetFamilyDashboardQuery : IRequest<Result<FamilyDashboardResponse>>;

public record FamilyDashboardResponse(
    Guid FamilyId,
    string FamilyName,
    List<KidSummaryDto> Kids,
    int TotalPendingTasks,
    int TotalPendingMoneyRequests,
    decimal TotalKidsBalance);

public record KidSummaryDto(
    Guid KidId,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    decimal MainAccountBalance,
    int ActiveGoals,
    int PendingTasks,
    int CompletedTasksThisWeek,
    int CurrentStreak,
    int TotalXp);

public class GetFamilyDashboardQueryHandler : IRequestHandler<GetFamilyDashboardQuery, Result<FamilyDashboardResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetFamilyDashboardQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FamilyDashboardResponse>> Handle(GetFamilyDashboardQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can access family dashboard");
        }

        if (!_currentUserService.FamilyId.HasValue)
        {
            return Error.InvalidOperation("User does not belong to a family");
        }

        var familyId = _currentUserService.FamilyId.Value;

        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family == null)
        {
            return Error.NotFound("Family", familyId);
        }

        var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + (int)DayOfWeek.Monday);

        var kids = await _context.Users
            .Where(u => u.FamilyId == familyId && u.Role == UserRole.Kid && !u.IsDeleted)
            .Select(k => new KidSummaryDto(
                k.Id,
                k.FirstName,
                k.LastName,
                k.AvatarUrl,
                k.Accounts.Where(a => a.Type == AccountType.Main && a.IsActive).Sum(a => a.Balance),
                k.WishlistGoals.Count(g => g.Status == GoalStatus.Active),
                k.AssignedTasks.Count(t => t.Status == TaskAssignmentStatus.Pending),
                k.AssignedTasks.Count(t => t.Status == TaskAssignmentStatus.Approved && t.ApprovedAt >= weekStart),
                k.CurrentStreak,
                k.TotalXp))
            .ToListAsync(cancellationToken);

        var totalPendingTasks = await _context.TaskAssignments
            .CountAsync(t => t.AssignedTo.FamilyId == familyId && t.Status == TaskAssignmentStatus.Completed, cancellationToken);

        var totalPendingRequests = await _context.MoneyRequests
            .CountAsync(mr => mr.Kid.FamilyId == familyId && mr.Status == MoneyRequestStatus.Pending, cancellationToken);

        var totalKidsBalance = kids.Sum(k => k.MainAccountBalance);

        return new FamilyDashboardResponse(
            familyId,
            family.Name,
            kids,
            totalPendingTasks,
            totalPendingRequests,
            totalKidsBalance);
    }
}
