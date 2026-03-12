using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using MediatR;

namespace KidBank.Application.Features.Goals.Commands;

public record CreateWishlistGoalCommand(
    string Title,
    decimal TargetAmount,
    string? Description = null,
    string? ImageUrl = null,
    DateTime? TargetDate = null) : IRequest<Result<GoalDto>>;

public record GoalDto(
    Guid Id,
    string Title,
    string? Description,
    string? ImageUrl,
    decimal TargetAmount,
    decimal CurrentAmount,
    string Currency,
    DateTime? TargetDate,
    string Status,
    decimal ProgressPercentage,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public class CreateWishlistGoalCommandValidator : AbstractValidator<CreateWishlistGoalCommand>
{
    public CreateWishlistGoalCommandValidator()
    {
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

public class CreateWishlistGoalCommandHandler : IRequestHandler<CreateWishlistGoalCommand, Result<GoalDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public CreateWishlistGoalCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<GoalDto>> Handle(CreateWishlistGoalCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var goal = GoalService.Create(
            _currentUserService.UserId.Value,
            request.Title,
            request.TargetAmount,
            "RUB",
            request.Description,
            request.ImageUrl,
            request.TargetDate);

        _context.WishlistGoals.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);

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
