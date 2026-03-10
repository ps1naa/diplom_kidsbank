using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Accounts.Queries;

public record GetKidAccountsQuery(Guid KidId) : IRequest<Result<List<AccountDto>>>;

public class GetKidAccountsQueryHandler : IRequestHandler<GetKidAccountsQuery, Result<List<AccountDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetKidAccountsQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AccountDto>>> Handle(GetKidAccountsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view kid's accounts");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("Kid", request.KidId);
        }

        if (kid.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot access accounts of kids from other families");
        }

        var accounts = await _context.Accounts
            .Where(a => a.UserId == request.KidId)
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
