using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.MoneyRequests.Commands;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.MoneyRequests.Queries;

public record GetPendingMoneyRequestsQuery : IRequest<Result<List<MoneyRequestDto>>>;

public class GetPendingMoneyRequestsQueryHandler : IRequestHandler<GetPendingMoneyRequestsQuery, Result<List<MoneyRequestDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetPendingMoneyRequestsQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MoneyRequestDto>>> Handle(GetPendingMoneyRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view pending money requests");
        }

        var entities = await _context.MoneyRequests
            .Include(mr => mr.Kid)
            .Include(mr => mr.Parent)
            .Where(mr => mr.ParentId == _currentUserService.UserId!.Value && mr.Status == MoneyRequestStatus.Pending)
            .OrderByDescending(mr => mr.CreatedAt)
            .ToListAsync(cancellationToken);

        var requests = entities.Select(mr => new MoneyRequestDto(
            mr.Id,
            mr.KidId,
            mr.Kid.FirstName + " " + mr.Kid.LastName,
            mr.ParentId,
            mr.Parent.FirstName + " " + mr.Parent.LastName,
            mr.Amount,
            mr.Currency,
            mr.Reason,
            mr.Status.ToString(),
            mr.ResponseNote,
            mr.CreatedAt,
            mr.RespondedAt)).ToList();

        return requests;
    }
}
