using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Accounts.Queries;
using KidBank.Domain.Entities;
using MediatR;

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

    public CreateSavingsAccountCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AccountDto>> Handle(CreateSavingsAccountCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var account = Account.CreateSavings(_currentUserService.UserId.Value, request.Name);

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
