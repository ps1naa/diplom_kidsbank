using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Cards.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Cards.Queries;

public record GetMyCardsQuery : IRequest<Result<List<VirtualCardDto>>>;

public class GetMyCardsQueryHandler : IRequestHandler<GetMyCardsQuery, Result<List<VirtualCardDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyCardsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<VirtualCardDto>>> Handle(GetMyCardsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return Error.Unauthorized();

        var cards = await _context.VirtualCards
            .Include(c => c.Account)
            .Where(c => c.Account.UserId == _currentUserService.UserId.Value && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new VirtualCardDto(
                c.Id,
                c.MaskedCardNumber,
                c.CardHolderName,
                c.ExpiryDate,
                c.IsActive,
                c.IsFrozen,
                c.DailyLimit,
                c.MonthlyLimit,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return cards;
    }
}
