using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.MoneyRequests.Commands;

public record ApproveMoneyRequestCommand(
    Guid RequestId,
    string? Note = null) : IRequest<Result<MoneyRequestDto>>;

public class ApproveMoneyRequestCommandValidator : AbstractValidator<ApproveMoneyRequestCommand>
{
    public ApproveMoneyRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Request ID is required");
    }
}

public class ApproveMoneyRequestCommandHandler : IRequestHandler<ApproveMoneyRequestCommand, Result<MoneyRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly LedgerService _ledgerService;

    public ApproveMoneyRequestCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        LedgerService ledgerService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _ledgerService = ledgerService;
    }

    public async Task<Result<MoneyRequestDto>> Handle(ApproveMoneyRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can approve money requests");
        }

        var moneyRequest = await _context.MoneyRequests
            .Include(mr => mr.Kid)
            .Include(mr => mr.Parent)
            .FirstOrDefaultAsync(mr => mr.Id == request.RequestId, cancellationToken);

        if (moneyRequest == null)
        {
            return Error.NotFound("Money request", request.RequestId);
        }

        if (moneyRequest.ParentId != _currentUserService.UserId)
        {
            return Error.Forbidden("You can only approve requests directed to you");
        }

        if (moneyRequest.Status != MoneyRequestStatus.Pending)
        {
            return Error.InvalidOperation("Can only approve pending requests");
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

        var kidAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => 
                a.UserId == moneyRequest.KidId && 
                a.Type == AccountType.Main && 
                a.IsActive, 
                cancellationToken);

        if (kidAccount == null)
        {
            return Error.NotFound("Kid main account not found");
        }

        if (parentAccount.Balance < moneyRequest.Amount)
        {
            return Error.InsufficientFunds();
        }

        try
        {
            var transaction = _ledgerService.TransferMoneyRequest(
                parentAccount,
                kidAccount,
                moneyRequest.Amount,
                moneyRequest.Id);

            _context.Transactions.Add(transaction);
            MoneyRequestService.Approve(moneyRequest, request.Note);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.ConcurrencyConflict();
        }

        return new MoneyRequestDto(
            moneyRequest.Id,
            moneyRequest.KidId,
            $"{moneyRequest.Kid.FirstName} {moneyRequest.Kid.LastName}",
            moneyRequest.ParentId,
            $"{moneyRequest.Parent.FirstName} {moneyRequest.Parent.LastName}",
            moneyRequest.Amount,
            moneyRequest.Currency,
            moneyRequest.Reason,
            moneyRequest.Status.ToString(),
            moneyRequest.ResponseNote,
            moneyRequest.CreatedAt,
            moneyRequest.RespondedAt);
    }
}
