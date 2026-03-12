using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Auth.Commands;

public record RegisterKidCommand(
    string InvitationToken,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime DateOfBirth) : IRequest<Result<AuthResponse>>;

public class RegisterKidCommandValidator : AbstractValidator<RegisterKidCommand>
{
    public RegisterKidCommandValidator()
    {
        RuleFor(x => x.InvitationToken)
            .NotEmpty().WithMessage("Invitation token is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeUnder18YearsOld).WithMessage("Kid must be under 18 years old");
    }

    private static bool BeUnder18YearsOld(DateTime dateOfBirth)
    {
        return dateOfBirth > DateTime.UtcNow.AddYears(-18);
    }
}

public class RegisterKidCommandHandler : IRequestHandler<RegisterKidCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IDataEncryptor _encryptor;

    public RegisterKidCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IDataEncryptor encryptor)
    {
        _context = context;
        _identityService = identityService;
        _encryptor = encryptor;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterKidCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _context.FamilyInvitations
            .FirstOrDefaultAsync(i => i.Token == request.InvitationToken, cancellationToken);

        if (invitation == null)
        {
            return Error.NotFound("Invitation not found or invalid");
        }

        if (!invitation.IsValid)
        {
            return Error.InvalidOperation("Invitation has expired or already been used");
        }

        var emailHash = _encryptor.ComputeHash(request.Email.ToLowerInvariant());
        var emailExists = await _context.Users
            .AnyAsync(u => u.NormalizedEmailHash == emailHash, cancellationToken);

        if (emailExists)
        {
            return Error.Conflict("A user with this email already exists");
        }

        var passwordHash = _identityService.HashPassword(request.Password);

        var user = UserService.CreateKid(
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            invitation.FamilyId,
            emailHash);

        FamilyInvitationService.MarkAsUsed(invitation, user.Id);

        var mainAccount = AccountService.CreateMain(user.Id);

        var (accessToken, jwtId) = _identityService.GenerateAccessToken(user);

        var refreshToken = RefreshTokenService.Create(
            user.Id,
            jwtId,
            30);

        _context.Users.Add(user);
        _context.Accounts.Add(mainAccount);
        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            invitation.FamilyId,
            accessToken,
            refreshToken.Token);
    }
}
