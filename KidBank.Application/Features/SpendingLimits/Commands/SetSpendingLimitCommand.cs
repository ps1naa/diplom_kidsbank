using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.SpendingLimits.Commands;

public record SetSpendingLimitCommand(
    Guid KidId,
    decimal LimitAmount,
    string Period) : IRequest<Result<SpendingLimitDto>>;

public record SpendingLimitDto(
    Guid Id,
    Guid KidId,
    string KidName,
    decimal LimitAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    string Currency,
    string Period,
    DateTime PeriodStartDate,
    DateTime PeriodEndDate,
    bool IsActive);

public class SetSpendingLimitCommandValidator : AbstractValidator<SetSpendingLimitCommand>
{
    public SetSpendingLimitCommandValidator()
    {
        RuleFor(x => x.KidId)
            .NotEmpty().WithMessage("Kid ID is required");

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Limit amount must be greater than zero");

        RuleFor(x => x.Period)
            .NotEmpty().WithMessage("Period is required")
            .Must(BeValidPeriod).WithMessage("Invalid period. Must be Daily, Weekly, or Monthly");
    }

    private static bool BeValidPeriod(string period)
    {
        return Enum.TryParse<SpendingLimitPeriod>(period, true, out _);
    }
}

public class SetSpendingLimitCommandHandler : IRequestHandler<SetSpendingLimitCommand, Result<SpendingLimitDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public SetSpendingLimitCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SpendingLimitDto>> Handle(SetSpendingLimitCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can set spending limits");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("Kid", request.KidId);
        }

        if (kid.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot set limits for kids from other families");
        }

        var period = Enum.Parse<SpendingLimitPeriod>(request.Period, true);

        var existingLimit = await _context.SpendingLimits
            .FirstOrDefaultAsync(sl => 
                sl.KidId == request.KidId && 
                sl.Period == period && 
                sl.IsActive, 
                cancellationToken);

        if (existingLimit != null)
        {
            SpendingLimitHelper.UpdateLimit(existingLimit, request.LimitAmount);
        }
        else
        {
            existingLimit = SpendingLimit.Create(
                request.KidId,
                _currentUserService.UserId!.Value,
                request.LimitAmount,
                period);

            _context.SpendingLimits.Add(existingLimit);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SpendingLimitDto(
            existingLimit.Id,
            existingLimit.KidId,
            $"{kid.FirstName} {kid.LastName}",
            existingLimit.LimitAmount,
            existingLimit.SpentAmount,
            SpendingLimitHelper.GetRemainingLimit(existingLimit),
            existingLimit.Currency,
            existingLimit.Period.ToString(),
            existingLimit.PeriodStartDate,
            existingLimit.PeriodEndDate,
            existingLimit.IsActive);
    }
}
