using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Cards.Commands;

public record FreezeCardCommand(Guid CardId) : IRequest<Result>;

public class FreezeCardCommandHandler : IRequestHandler<FreezeCardCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public FreezeCardCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(FreezeCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.VirtualCards
            .Include(c => c.Account)
            .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            return Error.NotFound("Card", request.CardId);

        var isOwner = card.Account.UserId == _currentUserService.UserId;
        var isParentOfOwner = _currentUserService.IsParent && 
                              card.Account.User.FamilyId == _currentUserService.FamilyId;

        if (!isOwner && !isParentOfOwner)
            return Error.Forbidden("Cannot manage this card");

        CardService.Freeze(card);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public record UnfreezeCardCommand(Guid CardId) : IRequest<Result>;

public class UnfreezeCardCommandHandler : IRequestHandler<UnfreezeCardCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public UnfreezeCardCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UnfreezeCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.VirtualCards
            .Include(c => c.Account)
            .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(c => c.Id == request.CardId, cancellationToken);

        if (card == null)
            return Error.NotFound("Card", request.CardId);

        var isOwner = card.Account.UserId == _currentUserService.UserId;
        var isParentOfOwner = _currentUserService.IsParent && 
                              card.Account.User.FamilyId == _currentUserService.FamilyId;

        if (!isOwner && !isParentOfOwner)
            return Error.Forbidden("Cannot manage this card");

        CardService.Unfreeze(card);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
