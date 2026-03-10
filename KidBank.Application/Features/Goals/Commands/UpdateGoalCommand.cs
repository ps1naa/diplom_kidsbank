using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Goals.Commands;

public record UpdateGoalCommand(
    Guid GoalId,
    string Title,
    decimal TargetAmount,
    string? Description = null,
    string? ImageUrl = null,
    DateTime? TargetDate = null) : IRequest<Result<GoalDto>>;

public class UpdateGoalCommandValidator : AbstractValidator<UpdateGoalCommand>
{
    public UpdateGoalCommandValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("Goal ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Goal title is required")
            .MaximumLength(200).WithMessage("Goal title must not exceed 200 characters");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than zero");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Target date must be in the future")
            .When(x => x.TargetDate.HasValue);
    }
}

public class UpdateGoalCommandHandler : IRequestHandler<UpdateGoalCommand, Result<GoalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public UpdateGoalCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GoalDto>> Handle(UpdateGoalCommand request, CancellationToken cancellationToken)
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
            return Error.Forbidden("You can only update your own goals");
        }

        try
        {
            GoalService.Update(goal, request.Title, request.Description, request.ImageUrl, request.TargetAmount, request.TargetDate);
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
            GoalService.GetProgress(goal),
            goal.CreatedAt,
            goal.CompletedAt);
    }
}
