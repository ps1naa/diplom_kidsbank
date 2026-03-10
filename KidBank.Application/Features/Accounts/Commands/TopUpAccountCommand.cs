using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Accounts.Commands;

public record TopUpAccountCommand(decimal Amount) : IRequest<Result<TopUpResultDto>>;

public record TopUpResultDto(decimal NewBalance, Guid TransactionId);

public class TopUpAccountCommandValidator : AbstractValidator<TopUpAccountCommand>
{
    public TopUpAccountCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(1000000).WithMessage("Amount too large");
    }
}

public class TopUpAccountCommandHandler : IRequestHandler<TopUpAccountCommand, Result<TopUpResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly LedgerService _ledgerService;

    public TopUpAccountCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        LedgerService ledgerService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ledgerService = ledgerService;
    }

    public async Task<Result<TopUpResultDto>> Handle(TopUpAccountCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => 
                a.UserId == _currentUserService.UserId.Value && 
                a.Type == AccountType.Main && 
                a.IsActive, 
                cancellationToken);

        if (account == null)
        {
            return Error.NotFound("Main account not found");
        }

        try
        {
            var transaction = _ledgerService.Deposit(account, request.Amount, "Пополнение счёта");
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return new TopUpResultDto(account.Balance, transaction.Id);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.ConcurrencyConflict();
        }
    }
}
