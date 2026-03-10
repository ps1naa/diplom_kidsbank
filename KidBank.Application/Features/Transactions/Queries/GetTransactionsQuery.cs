using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Transactions.Queries;

public record GetTransactionsQuery(
    Guid AccountId,
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Type = null) : IRequest<Result<PaginatedList<TransactionDto>>>;

public record TransactionDto(
    Guid Id,
    Guid? SourceAccountId,
    Guid? DestinationAccountId,
    string Type,
    decimal Amount,
    string Currency,
    string? Description,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    DateTime CreatedAt);

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, Result<PaginatedList<TransactionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetTransactionsQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<TransactionDto>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var account = await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
        {
            return Error.NotFound("Account", request.AccountId);
        }

        var canAccess = account.UserId == _currentUserService.UserId.Value ||
                        (_currentUserService.IsParent && account.User.FamilyId == _currentUserService.FamilyId);

        if (!canAccess)
        {
            return Error.Forbidden("Cannot access transactions from this account");
        }

        var query = _context.Transactions
            .Where(t => t.SourceAccountId == request.AccountId || t.DestinationAccountId == request.AccountId);

        if (request.FromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(request.Type) && Enum.TryParse<Domain.Enums.TransactionType>(request.Type, true, out var transactionType))
        {
            query = query.Where(t => t.Type == transactionType);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.SourceAccountId,
                t.DestinationAccountId,
                t.Type.ToString(),
                t.Amount,
                t.Currency,
                t.Description,
                t.RelatedEntityId,
                t.RelatedEntityType,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedList<TransactionDto>(transactions, totalCount, request.PageNumber, request.PageSize);
    }
}
