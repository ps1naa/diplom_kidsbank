using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string? DeviceInfo = null,
    string? IpAddress = null) : IRequest<Result<AuthResponse>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IDataEncryptor _encryptor;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IDataEncryptor encryptor)
    {
        _context = context;
        _identityService = identityService;
        _encryptor = encryptor;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailHash = _encryptor.ComputeHash(request.Email.ToLowerInvariant());
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmailHash == emailHash && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Error.Unauthorized("Invalid email or password");
        }

        if (!_identityService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Error.Unauthorized("Invalid email or password");
        }

        var (accessToken, jwtId) = _identityService.GenerateAccessToken(user);

        var refreshToken = RefreshTokenService.Create(
            user.Id,
            jwtId,
            30,
            request.DeviceInfo,
            request.IpAddress);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.FamilyId,
            accessToken,
            refreshToken.Token);
    }
}
