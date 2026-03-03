using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Accounts.Queries;

public record GetMyAccountsQuery : IRequest<Result<List<AccountDto>>>;

public record AccountDto(
    Guid Id,
    string Name,
    string Type,
    decimal Balance,
    string Currency,
    bool IsActive,
    DateTime CreatedAt);

public class GetMyAccountsQueryHandler : IRequestHandler<GetMyAccountsQuery, Result<List<AccountDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAccountsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AccountDto>>> Handle(GetMyAccountsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var accounts = await _context.Accounts
            .Where(a => a.UserId == _currentUserService.UserId.Value && a.IsActive)
            .Select(a => new AccountDto(
                a.Id,
                a.Name,
                a.Type.ToString(),
                a.Balance,
                a.Currency,
                a.IsActive,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return accounts;
    }
}
