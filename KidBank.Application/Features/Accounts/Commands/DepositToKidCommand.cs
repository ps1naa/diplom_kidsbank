using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Accounts.Commands;

public record DepositToKidCommand(
    Guid KidId,
    decimal Amount,
    string? Description = null) : IRequest<Result<TransactionResultDto>>;

public record TransactionResultDto(
    Guid TransactionId,
    decimal Amount,
    decimal NewBalance,
    DateTime CreatedAt);

public class DepositToKidCommandValidator : AbstractValidator<DepositToKidCommand>
{
    public DepositToKidCommandValidator()
    {
        RuleFor(x => x.KidId)
            .NotEmpty().WithMessage("Kid ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}

public class DepositToKidCommandHandler : IRequestHandler<DepositToKidCommand, Result<TransactionResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly LedgerService _ledgerService;

    public DepositToKidCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        LedgerService ledgerService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ledgerService = ledgerService;
    }

    public async Task<Result<TransactionResultDto>> Handle(DepositToKidCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can deposit to kid accounts");
        }

        var parentAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => 
                a.UserId == _currentUserService.UserId!.Value && 
                a.Type == AccountType.Main && 
                a.IsActive, 
                cancellationToken);

        if (parentAccount == null)
        {
            return Error.NotFound("Parent main account not found");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && u.Role == UserRole.Kid && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("Kid", request.KidId);
        }

        if (kid.FamilyId != _currentUserService.FamilyId)
        {
            return Error.Forbidden("Cannot deposit to kids from other families");
        }

        var kidAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => 
                a.UserId == request.KidId && 
                a.Type == AccountType.Main && 
                a.IsActive, 
                cancellationToken);

        if (kidAccount == null)
        {
            return Error.NotFound("Kid main account not found");
        }

        if (parentAccount.Balance < request.Amount)
        {
            return Error.InsufficientFunds();
        }

        try
        {
            var transaction = _ledgerService.Transfer(
                parentAccount, 
                kidAccount, 
                request.Amount, 
                request.Description ?? "Пополнение от родителя");

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return new TransactionResultDto(
                transaction.Id,
                transaction.Amount,
                kidAccount.Balance,
                transaction.CreatedAt);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.ConcurrencyConflict();
        }
    }
}
