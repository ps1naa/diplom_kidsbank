using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Goals.Commands;

public record DeleteGoalCommand(Guid GoalId) : IRequest<Result>;

public class DeleteGoalCommandValidator : AbstractValidator<DeleteGoalCommand>
{
    public DeleteGoalCommandValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("Goal ID is required");
    }
}

public class DeleteGoalCommandHandler : IRequestHandler<DeleteGoalCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly LedgerService _ledgerService;

    public DeleteGoalCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        LedgerService ledgerService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ledgerService = ledgerService;
    }

    public async Task<Result> Handle(DeleteGoalCommand request, CancellationToken cancellationToken)
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
            return Error.Forbidden("You can only delete your own goals");
        }

        if (goal.Status != GoalStatus.Active)
        {
            return Error.InvalidOperation("Only active goals can be deleted");
        }

        if (goal.CurrentAmount > 0)
        {
            var mainAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == goal.UserId && a.Type == AccountType.Main, cancellationToken);

            if (mainAccount == null)
                return Error.NotFound("Main account not found");

            var tx = _ledgerService.WithdrawFromGoal(goal, mainAccount, goal.CurrentAmount);
            _context.Transactions.Add(tx);
        }

        GoalService.Cancel(goal);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
