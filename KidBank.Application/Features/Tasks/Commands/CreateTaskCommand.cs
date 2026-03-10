using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Tasks.Commands;

public record CreateTaskCommand(
    Guid AssignedToId,
    string Title,
    decimal RewardAmount,
    string? Description = null,
    DateTime? DueDate = null) : IRequest<Result<TaskDto>>;

public record TaskDto(
    Guid Id,
    Guid AssignedToId,
    string AssignedToName,
    Guid CreatedById,
    string CreatedByName,
    string Title,
    string? Description,
    decimal RewardAmount,
    string Currency,
    DateTime? DueDate,
    string Status,
    string? ProofUrl,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    DateTime? ApprovedAt);

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.AssignedToId)
            .NotEmpty().WithMessage("Kid ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.RewardAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Reward amount cannot be negative");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future")
            .When(x => x.DueDate.HasValue);
    }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public CreateTaskCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can create tasks");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AssignedToId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("Kid", request.AssignedToId);
        }

        if (kid.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot create tasks for kids from other families");
        }

        var parent = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId!.Value, cancellationToken);

        var task = TaskAssignment.Create(
            request.AssignedToId,
            _currentUserService.UserId!.Value,
            request.Title,
            request.RewardAmount,
            "RUB",
            request.Description,
            request.DueDate);

        _context.TaskAssignments.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return new TaskDto(
            task.Id,
            task.AssignedToId,
            $"{kid.FirstName} {kid.LastName}",
            task.CreatedById,
            $"{parent!.FirstName} {parent.LastName}",
            task.Title,
            task.Description,
            task.RewardAmount,
            task.Currency,
            task.DueDate,
            task.Status.ToString(),
            task.ProofUrl,
            task.RejectionReason,
            task.CreatedAt,
            task.CompletedAt,
            task.ApprovedAt);
    }
}
