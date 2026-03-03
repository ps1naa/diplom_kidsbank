using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Allowances.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Allowances.Queries;

public record GetKidAllowanceQuery(Guid KidId) : IRequest<Result<AllowanceDto?>>;

public class GetKidAllowanceQueryHandler : IRequestHandler<GetKidAllowanceQuery, Result<AllowanceDto?>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetKidAllowanceQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AllowanceDto?>> Handle(GetKidAllowanceQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
            return Error.Forbidden("Only parents can view allowances");

        var allowance = await _context.RecurringAllowances
            .Include(a => a.Kid)
            .FirstOrDefaultAsync(a => a.KidId == request.KidId && a.IsActive, cancellationToken);

        if (allowance == null)
            return (AllowanceDto?)null;

        if (allowance.Kid.FamilyId != _currentUserService.FamilyId)
            return Error.Forbidden("Kid belongs to another family");

        return new AllowanceDto(
            allowance.Id,
            allowance.KidId,
            $"{allowance.Kid.FirstName} {allowance.Kid.LastName}",
            allowance.Amount,
            allowance.Currency,
            allowance.Frequency,
            allowance.DayOfWeek,
            allowance.DayOfMonth,
            allowance.NextPaymentDate,
            allowance.IsActive,
            allowance.CreatedAt);
    }
}
