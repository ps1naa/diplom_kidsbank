using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Accounts.Commands;

public record DeleteSavingsAccountCommand(Guid AccountId) : IRequest<Result>;

public class DeleteSavingsAccountCommandHandler : IRequestHandler<DeleteSavingsAccountCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly LedgerService _ledgerService;

    public DeleteSavingsAccountCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        LedgerService ledgerService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ledgerService = ledgerService;
    }

    public async Task<Result> Handle(DeleteSavingsAccountCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return Error.Unauthorized();

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.IsActive, cancellationToken);

        if (account == null)
            return Error.NotFound("Account", request.AccountId);

        if (account.UserId != _currentUserService.UserId.Value)
            return Error.Forbidden("You can only delete your own accounts");

        if (account.Type != AccountType.Savings)
            return Error.InvalidOperation("Only savings accounts can be deleted");

        if (account.Balance > 0)
        {
            var mainAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == account.UserId && a.Type == AccountType.Main && a.IsActive, cancellationToken);

            if (mainAccount == null)
                return Error.NotFound("Main account not found");

            var tx = _ledgerService.Transfer(account, mainAccount, account.Balance, "Закрытие копилки — возврат средств");
            _context.Transactions.Add(tx);
        }

        account.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
