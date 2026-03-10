using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Exceptions;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Tasks.Commands;

public record CompleteTaskCommand(
    Guid TaskId,
    string? ProofUrl = null) : IRequest<Result<TaskDto>>;

public class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    public CompleteTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");
    }
}

public class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, Result<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public CompleteTaskCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TaskDto>> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var task = await _context.TaskAssignments
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            return Error.NotFound("Task", request.TaskId);
        }

        if (task.AssignedToId != _currentUserService.UserId.Value)
        {
            return Error.Forbidden("You can only complete tasks assigned to you");
        }

        try
        {
            TaskService.MarkAsCompleted(task, request.ProofUrl);
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
