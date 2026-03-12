using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Tasks.Commands;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Tasks.Queries;

public record GetPendingApprovalTasksQuery : IRequest<Result<List<TaskDto>>>;

public class GetPendingApprovalTasksQueryHandler : IRequestHandler<GetPendingApprovalTasksQuery, Result<List<TaskDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetPendingApprovalTasksQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<TaskDto>>> Handle(GetPendingApprovalTasksQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view pending approval tasks");
        }

        var entities = await _context.TaskAssignments
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => t.CreatedById == _currentUserService.UserId!.Value && t.Status == TaskAssignmentStatus.Completed)
            .OrderByDescending(t => t.CompletedAt)
            .ToListAsync(cancellationToken);

        var tasks = entities.Select(t => new TaskDto(
            t.Id,
            t.AssignedToId,
            t.AssignedTo.FirstName + " " + t.AssignedTo.LastName,
            t.CreatedById,
            t.CreatedBy.FirstName + " " + t.CreatedBy.LastName,
            t.Title,
            t.Description,
            t.RewardAmount,
            t.Currency,
            t.DueDate,
            t.Status.ToString(),
            t.ProofUrl,
            t.RejectionReason,
            t.CreatedAt,
            t.CompletedAt,
            t.ApprovedAt)).ToList();

        return tasks;
    }
}
