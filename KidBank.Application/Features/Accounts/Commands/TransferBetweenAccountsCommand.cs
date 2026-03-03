using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Exceptions;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Accounts.Commands;

public record TransferBetweenAccountsCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string? Description = null) : IRequest<Result<TransactionResultDto>>;

public class TransferBetweenAccountsCommandValidator : AbstractValidator<TransferBetweenAccountsCommand>
{
    public TransferBetweenAccountsCommandValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty().WithMessage("Source account ID is required");

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty().WithMessage("Destination account ID is required")
            .NotEqual(x => x.SourceAccountId).WithMessage("Cannot transfer to the same account");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}

public class TransferBetweenAccountsCommandHandler : IRequestHandler<TransferBetweenAccountsCommand, Result<TransactionResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly LedgerService _ledgerService;
    private readonly SpendingValidationService _spendingValidationService;

    public TransferBetweenAccountsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        LedgerService ledgerService,
        SpendingValidationService spendingValidationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ledgerService = ledgerService;
        _spendingValidationService = spendingValidationService;
    }

    public async Task<Result<TransactionResultDto>> Handle(TransferBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var sourceAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.SourceAccountId && a.IsActive, cancellationToken);

        if (sourceAccount == null)
        {
            return Error.NotFound("Source account", request.SourceAccountId);
        }

        if (sourceAccount.UserId != _currentUserService.UserId.Value)
        {
            return Error.Forbidden("You can only transfer from your own accounts");
        }

        var destinationAccount = await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.DestinationAccountId && a.IsActive, cancellationToken);

        if (destinationAccount == null)
        {
            return Error.NotFound("Destination account", request.DestinationAccountId);
        }

        if (destinationAccount.User.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot transfer to accounts outside your family");
        }

        if (!sourceAccount.HasSufficientFunds(request.Amount))
        {
            return Error.InsufficientFunds();
        }

        if (_currentUserService.IsKid)
        {
            var limits = await _context.SpendingLimits
                .Where(sl => sl.KidId == _currentUserService.UserId.Value && sl.IsActive)
                .ToListAsync(cancellationToken);

            try
            {
                _spendingValidationService.ValidateSpending(limits, request.Amount);
                _spendingValidationService.RecordSpending(limits, request.Amount);
            }
            catch (SpendingLimitExceededException ex)
            {
                return Error.SpendingLimitExceeded(ex.Code == "SPENDING_LIMIT_EXCEEDED" ? 0 : 0, request.Amount);
            }
        }

        try
        {
            var transaction = _ledgerService.Transfer(
                sourceAccount,
                destinationAccount,
                request.Amount,
                request.Description ?? "Перевод между счетами");

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return new TransactionResultDto(
                transaction.Id,
                transaction.Amount,
                sourceAccount.Balance,
                transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.ConcurrencyConflict();
        }
    }
}
