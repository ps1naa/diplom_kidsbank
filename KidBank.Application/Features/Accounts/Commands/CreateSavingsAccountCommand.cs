using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Accounts.Queries;
using KidBank.Domain.Entities;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Accounts.Commands;

public record CreateSavingsAccountCommand(string Name) : IRequest<Result<AccountDto>>;

public class CreateSavingsAccountCommandValidator : AbstractValidator<CreateSavingsAccountCommand>
{
    public CreateSavingsAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Account name is required")
            .MaximumLength(100).WithMessage("Account name must not exceed 100 characters");
    }
}

public class CreateSavingsAccountCommandHandler : IRequestHandler<CreateSavingsAccountCommand, Result<AccountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly ISubscriptionService _subscription;

    public CreateSavingsAccountCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        ISubscriptionService subscription)
    {
        _context = context;
        _currentUserService = currentUserService;
        _subscription = subscription;
    }

    public async Task<Result<AccountDto>> Handle(CreateSavingsAccountCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue || !_currentUserService.FamilyId.HasValue)
        {
            return Error.Unauthorized();
        }

        var limits = await _subscription.GetLimitsAsync(_currentUserService.FamilyId.Value, cancellationToken);
        var savingsCount = await _context.Accounts
            .CountAsync(a => a.UserId == _currentUserService.UserId.Value && a.Type == AccountType.Savings && a.IsActive, cancellationToken);
        if (savingsCount >= limits.MaxSavingsAccounts)
            return Error.SubscriptionRequired($"Maximum {limits.MaxSavingsAccounts} savings accounts. Upgrade to Pro for up to 5");

        var account = AccountService.CreateSavings(_currentUserService.UserId.Value, request.Name);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);

        return new AccountDto(
            account.Id,
            account.Name,
            account.Type.ToString(),
            account.Balance,
            account.Currency,
            account.IsActive,
            account.CreatedAt);
    }
}
