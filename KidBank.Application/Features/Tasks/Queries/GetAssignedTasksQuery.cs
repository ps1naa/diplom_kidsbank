using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Tasks.Commands;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Tasks.Queries;

public record GetAssignedTasksQuery(string? Status = null) : IRequest<Result<List<TaskDto>>>;

public class GetAssignedTasksQueryHandler : IRequestHandler<GetAssignedTasksQuery, Result<List<TaskDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAssignedTasksQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<TaskDto>>> Handle(GetAssignedTasksQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var query = _context.TaskAssignments
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => t.AssignedToId == _currentUserService.UserId.Value);

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TaskAssignmentStatus>(request.Status, true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskDto(
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
                t.ApprovedAt))
            .ToListAsync(cancellationToken);

        return tasks;
    }
}
