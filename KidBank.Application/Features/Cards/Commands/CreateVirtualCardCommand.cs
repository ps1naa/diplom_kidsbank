using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Cards.Commands;

public record CreateVirtualCardCommand(
    Guid AccountId,
    string? CardName = null) : IRequest<Result<VirtualCardDto>>;

public record VirtualCardDto(
    Guid Id,
    string CardNumber,
    string CardHolderName,
    DateTime ExpiryDate,
    bool IsActive,
    bool IsFrozen,
    decimal? DailyLimit,
    decimal? MonthlyLimit,
    DateTime CreatedAt);

public class CreateVirtualCardCommandValidator : AbstractValidator<CreateVirtualCardCommand>
{
    public CreateVirtualCardCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
    }
}

public class CreateVirtualCardCommandHandler : IRequestHandler<CreateVirtualCardCommand, Result<VirtualCardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public CreateVirtualCardCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<VirtualCardDto>> Handle(CreateVirtualCardCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.IsActive, cancellationToken);

        if (account == null)
            return Error.NotFound("Account", request.AccountId);

        if (account.UserId != _currentUserService.UserId && !_currentUserService.IsParent)
            return Error.Forbidden("Cannot create card for this account");

        if (_currentUserService.IsParent && account.User.FamilyId != _currentUserService.FamilyId)
            return Error.Forbidden("Account belongs to another family");

        var cardHolderName = $"{account.User.FirstName} {account.User.LastName}".ToUpperInvariant();
        var card = VirtualCard.Create(request.AccountId, cardHolderName);

        _context.VirtualCards.Add(card);
        await _context.SaveChangesAsync(cancellationToken);

        return new VirtualCardDto(
            card.Id,
            card.MaskedCardNumber,
            card.CardHolderName,
            card.ExpiryDate,
            card.IsActive,
            card.IsFrozen,
            card.DailyLimit,
            card.MonthlyLimit,
            card.CreatedAt);
    }
}
