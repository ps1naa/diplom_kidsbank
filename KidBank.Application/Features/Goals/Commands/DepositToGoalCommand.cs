using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Goals.Commands;

public record DepositToGoalCommand(
    Guid GoalId,
    decimal Amount) : IRequest<Result<GoalDto>>;

public class DepositToGoalCommandValidator : AbstractValidator<DepositToGoalCommand>
{
    public DepositToGoalCommandValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("Goal ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}

public class DepositToGoalCommandHandler : IRequestHandler<DepositToGoalCommand, Result<GoalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly LedgerService _ledgerService;

    public DepositToGoalCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        LedgerService ledgerService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ledgerService = ledgerService;
    }

    public async Task<Result<GoalDto>> Handle(DepositToGoalCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var goal = await _context.WishlistGoals
            .FirstOrDefaultAsync(g => g.Id == request.GoalId, cancellationToken);

        if (goal == null)
        {
            return Error.NotFound("Goal", request.GoalId);
        }

        if (goal.UserId != _currentUserService.UserId.Value)
        {
            return Error.Forbidden("You can only deposit to your own goals");
        }

        if (goal.Status != GoalStatus.Active)
        {
            return Error.InvalidOperation("Can only deposit to active goals");
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => 
                a.UserId == _currentUserService.UserId.Value && 
                a.Type == AccountType.Main && 
                a.IsActive, 
                cancellationToken);

        if (account == null)
        {
            return Error.NotFound("Main account not found");
        }

        if (!account.HasSufficientFunds(request.Amount))
        {
            return Error.InsufficientFunds();
        }

        try
        {
            var transaction = _ledgerService.DepositToGoal(account, goal, request.Amount);
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.ConcurrencyConflict();
        }

        return new GoalDto(
            goal.Id,
            goal.Title,
            goal.Description,
            goal.ImageUrl,
            goal.TargetAmount,
            goal.CurrentAmount,
            goal.Currency,
            goal.TargetDate,
            goal.Status.ToString(),
            goal.GetProgress(),
            goal.CreatedAt,
            goal.CompletedAt);
    }
}
