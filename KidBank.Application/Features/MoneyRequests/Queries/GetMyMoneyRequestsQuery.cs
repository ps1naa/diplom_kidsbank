using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.MoneyRequests.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.MoneyRequests.Queries;

public record GetMyMoneyRequestsQuery : IRequest<Result<List<MoneyRequestDto>>>;

public class GetMyMoneyRequestsQueryHandler : IRequestHandler<GetMyMoneyRequestsQuery, Result<List<MoneyRequestDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyMoneyRequestsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MoneyRequestDto>>> Handle(GetMyMoneyRequestsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var requests = await _context.MoneyRequests
            .Include(mr => mr.Kid)
            .Include(mr => mr.Parent)
            .Where(mr => mr.KidId == _currentUserService.UserId.Value)
            .OrderByDescending(mr => mr.CreatedAt)
            .Select(mr => new MoneyRequestDto(
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
                mr.RespondedAt))
            .ToListAsync(cancellationToken);

        return requests;
    }
}
