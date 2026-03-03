using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Allowances.Commands;

public record SetAllowanceCommand(
    Guid KidId,
    decimal Amount,
    string Frequency,
    int DayOfWeek = 1,
    int DayOfMonth = 1) : IRequest<Result<AllowanceDto>>;

public record AllowanceDto(
    Guid Id,
    Guid KidId,
    string KidName,
    decimal Amount,
    string Currency,
    string Frequency,
    int DayOfWeek,
    int DayOfMonth,
    DateTime? NextPaymentDate,
    bool IsActive,
    DateTime CreatedAt);

public class SetAllowanceCommandValidator : AbstractValidator<SetAllowanceCommand>
{
    public SetAllowanceCommandValidator()
    {
        RuleFor(x => x.KidId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Frequency).Must(f => new[] { "Weekly", "BiWeekly", "Monthly" }.Contains(f));
        RuleFor(x => x.DayOfWeek).InclusiveBetween(0, 6);
        RuleFor(x => x.DayOfMonth).InclusiveBetween(1, 28);
    }
}

public class SetAllowanceCommandHandler : IRequestHandler<SetAllowanceCommand, Result<AllowanceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SetAllowanceCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AllowanceDto>> Handle(SetAllowanceCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
            return Error.Forbidden("Only parents can set allowances");

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
            return Error.NotFound("Kid", request.KidId);

        if (kid.FamilyId != _currentUserService.FamilyId)
            return Error.Forbidden("Kid belongs to another family");

        var existingAllowance = await _context.RecurringAllowances
            .FirstOrDefaultAsync(a => a.KidId == request.KidId && a.IsActive, cancellationToken);

        if (existingAllowance != null)
        {
            existingAllowance.Update(request.Amount, request.Frequency, request.DayOfWeek, request.DayOfMonth);
        }
        else
        {
            existingAllowance = Domain.Entities.RecurringAllowance.Create(
                _currentUserService.UserId!.Value,
                request.KidId,
                request.Amount,
                "RUB",
                request.Frequency,
                request.DayOfWeek,
                request.DayOfMonth);
            _context.RecurringAllowances.Add(existingAllowance);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new AllowanceDto(
            existingAllowance.Id,
            existingAllowance.KidId,
            $"{kid.FirstName} {kid.LastName}",
            existingAllowance.Amount,
            existingAllowance.Currency,
            existingAllowance.Frequency,
            existingAllowance.DayOfWeek,
            existingAllowance.DayOfMonth,
            existingAllowance.NextPaymentDate,
            existingAllowance.IsActive,
            existingAllowance.CreatedAt);
    }
}
