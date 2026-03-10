using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Exceptions;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Tasks.Commands;

public record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    decimal RewardAmount,
    string? Description = null,
    DateTime? DueDate = null) : IRequest<Result<TaskDto>>;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters");

        RuleFor(x => x.RewardAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Reward amount cannot be negative");
    }
}

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public UpdateTaskCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can update tasks");
        }

        var task = await _context.TaskAssignments
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            return Error.NotFound("Task", request.TaskId);
        }

        if (task.CreatedById != _currentUserService.UserId)
        {
            return Error.Forbidden("You can only update tasks you created");
        }

        try
        {
            TaskService.Update(task, request.Title, request.Description, request.RewardAmount, request.DueDate);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DomainException ex) when (ex.Type == ErrorType.InvalidOperation)
        {
            return Error.InvalidOperation(ex.Message);
        }

        return new TaskDto(
            task.Id,
            task.AssignedToId,
            $"{task.AssignedTo.FirstName} {task.AssignedTo.LastName}",
            task.CreatedById,
            $"{task.CreatedBy.FirstName} {task.CreatedBy.LastName}",
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
