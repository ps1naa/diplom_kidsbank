using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.SpendingLimits.Commands;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.SpendingLimits.Queries;

public record GetSpendingLimitsQuery(Guid KidId) : IRequest<Result<List<SpendingLimitDto>>>;

public class GetSpendingLimitsQueryHandler : IRequestHandler<GetSpendingLimitsQuery, Result<List<SpendingLimitDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetSpendingLimitsQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<SpendingLimitDto>>> Handle(GetSpendingLimitsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view spending limits");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("Kid", request.KidId);
        }

        if (kid.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot view limits for kids from other families");
        }

        var limits = await _context.SpendingLimits
            .Include(sl => sl.Kid)
            .Where(sl => sl.KidId == request.KidId && sl.IsActive)
            .Select(sl => new SpendingLimitDto(
                sl.Id,
                sl.KidId,
                sl.Kid.FirstName + " " + sl.Kid.LastName,
                sl.LimitAmount,
                sl.SpentAmount,
                sl.LimitAmount - sl.SpentAmount,
                sl.Currency,
                sl.Period.ToString(),
                sl.PeriodStartDate,
                sl.PeriodEndDate,
                sl.IsActive))
            .ToListAsync(cancellationToken);

        return limits;
    }
}
