using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Error.Unauthorized("Invalid email or password");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Error.Unauthorized("Invalid email or password");
        }

        var (accessToken, jwtId) = _jwtService.GenerateAccessToken(user);

        var refreshToken = RefreshToken.Create(
            user.Id,
            jwtId,
            TimeSpan.FromDays(30),
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
