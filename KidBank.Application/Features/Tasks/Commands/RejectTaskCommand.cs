using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Tasks.Commands;

public record RejectTaskCommand(
    Guid TaskId,
    string? Reason = null) : IRequest<Result<TaskDto>>;

public class RejectTaskCommandValidator : AbstractValidator<RejectTaskCommand>
{
    public RejectTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");
    }
}

public class RejectTaskCommandHandler : IRequestHandler<RejectTaskCommand, Result<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RejectTaskCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TaskDto>> Handle(RejectTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can reject tasks");
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
            return Error.Forbidden("You can only reject tasks you created");
        }

        if (task.Status != TaskAssignmentStatus.Completed)
        {
            return Error.InvalidOperation("Can only reject completed tasks");
        }

        task.Reject(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);

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
