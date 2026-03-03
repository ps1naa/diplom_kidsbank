using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.SpendingLimits.Commands;

public record UpdateSpendingLimitCommand(
    Guid LimitId,
    decimal LimitAmount) : IRequest<Result<SpendingLimitDto>>;

public class UpdateSpendingLimitCommandValidator : AbstractValidator<UpdateSpendingLimitCommand>
{
    public UpdateSpendingLimitCommandValidator()
    {
        RuleFor(x => x.LimitId)
            .NotEmpty().WithMessage("Limit ID is required");

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Limit amount must be greater than zero");
    }
}

public class UpdateSpendingLimitCommandHandler : IRequestHandler<UpdateSpendingLimitCommand, Result<SpendingLimitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSpendingLimitCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SpendingLimitDto>> Handle(UpdateSpendingLimitCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can update spending limits");
        }

        var limit = await _context.SpendingLimits
            .Include(sl => sl.Kid)
            .FirstOrDefaultAsync(sl => sl.Id == request.LimitId, cancellationToken);

        if (limit == null)
        {
            return Error.NotFound("Spending limit", request.LimitId);
        }

        if (limit.SetById != _currentUserService.UserId)
        {
            return Error.Forbidden("You can only update limits you set");
        }

        try
        {
            limit.UpdateLimit(request.LimitAmount);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.ConcurrencyConflict();
        }

        return new SpendingLimitDto(
            limit.Id,
            limit.KidId,
            $"{limit.Kid.FirstName} {limit.Kid.LastName}",
            limit.LimitAmount,
            limit.SpentAmount,
            limit.GetRemainingLimit(),
            limit.Currency,
            limit.Period.ToString(),
            limit.PeriodStartDate,
            limit.PeriodEndDate,
            limit.IsActive);
    }
}
