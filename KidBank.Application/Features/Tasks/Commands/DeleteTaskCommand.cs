using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Tasks.Commands;

public record DeleteTaskCommand(Guid TaskId) : IRequest<Result>;

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");
    }
}

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTaskCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can delete tasks");
        }

        var task = await _context.TaskAssignments
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            return Error.NotFound("Task", request.TaskId);
        }

        if (task.CreatedById != _currentUserService.UserId)
        {
            return Error.Forbidden("You can only delete tasks you created");
        }

        if (task.Status == TaskAssignmentStatus.Approved)
        {
            return Error.InvalidOperation("Cannot delete approved tasks");
        }

        _context.TaskAssignments.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
