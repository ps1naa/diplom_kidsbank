using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Auth.Commands;

public record RevokeAllTokensCommand : IRequest<Result>;

public class RevokeAllTokensCommandHandler : IRequestHandler<RevokeAllTokensCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RevokeAllTokensCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RevokeAllTokensCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == _currentUserService.UserId.Value && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
