using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Tasks.Commands;

public record ApproveTaskCommand(Guid TaskId) : IRequest<Result<TaskDto>>;

public class ApproveTaskCommandValidator : AbstractValidator<ApproveTaskCommand>
{
    public ApproveTaskCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required");
    }
}

public class ApproveTaskCommandHandler : IRequestHandler<ApproveTaskCommand, Result<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly LedgerService _ledgerService;
    private readonly GamificationService _gamificationService;

    public ApproveTaskCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        LedgerService ledgerService,
        GamificationService gamificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ledgerService = ledgerService;
        _gamificationService = gamificationService;
    }

    public async Task<Result<TaskDto>> Handle(ApproveTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can approve tasks");
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
            return Error.Forbidden("You can only approve tasks you created");
        }

        if (task.Status != TaskAssignmentStatus.Completed)
        {
            return Error.InvalidOperation("Can only approve completed tasks");
        }

        if (task.RewardAmount > 0)
        {
            var parentAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => 
                    a.UserId == _currentUserService.UserId!.Value && 
                    a.Type == AccountType.Main && 
                    a.IsActive, 
                    cancellationToken);

            if (parentAccount == null)
            {
                return Error.NotFound("Parent main account not found");
            }

            var kidAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => 
                    a.UserId == task.AssignedToId && 
                    a.Type == AccountType.Main && 
                    a.IsActive, 
                    cancellationToken);

            if (kidAccount == null)
            {
                return Error.NotFound("Kid main account not found");
            }

            if (!parentAccount.HasSufficientFunds(task.RewardAmount))
            {
                return Error.InsufficientFunds();
            }

            try
            {
                var transaction = _ledgerService.TransferTaskReward(
                    parentAccount,
                    kidAccount,
                    task.RewardAmount,
                    task.Id);

                _context.Transactions.Add(transaction);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Error.ConcurrencyConflict();
            }
        }

        task.Approve();
        _gamificationService.AwardXpForTaskCompletion(task.AssignedTo, 50);

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
