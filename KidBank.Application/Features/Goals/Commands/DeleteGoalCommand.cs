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

    public DeleteGoalCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
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
            return Error.InvalidOperation("Cannot delete goal with accumulated funds. Withdraw funds first.");
        }

        GoalService.Cancel(goal);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
