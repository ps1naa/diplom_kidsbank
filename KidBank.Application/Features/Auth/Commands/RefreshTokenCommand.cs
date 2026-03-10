using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Auth.Commands;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken) : IRequest<Result<TokenResponse>>;

public record TokenResponse(
    string AccessToken,
    string RefreshToken);

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (!_identityService.ValidateAccessToken(request.AccessToken, out var jwtId))
        {
            return Error.Unauthorized("Invalid access token");
        }

        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (storedToken == null)
        {
            return Error.Unauthorized("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            return Error.Unauthorized("Refresh token is no longer active");
        }

        if (storedToken.JwtId != jwtId)
        {
            return Error.Unauthorized("Token mismatch");
        }

        var user = storedToken.User;
        if (user.IsDeleted)
        {
            return Error.Unauthorized("User account is deactivated");
        }

        var (newAccessToken, newJwtId) = _identityService.GenerateAccessToken(user);

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newJwtId,
            TimeSpan.FromDays(30));

        storedToken.Revoke(newRefreshToken.Token);

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokenResponse(newAccessToken, newRefreshToken.Token);
    }
}
