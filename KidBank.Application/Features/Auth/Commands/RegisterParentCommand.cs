using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Auth.Commands;

public record RegisterParentCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string FamilyName) : IRequest<Result<AuthResponse>>;

public record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    Guid FamilyId,
    string AccessToken,
    string RefreshToken);

public class RegisterParentCommandValidator : AbstractValidator<RegisterParentCommand>
{
    public RegisterParentCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeAtLeast18YearsOld).WithMessage("Parent must be at least 18 years old");

        RuleFor(x => x.FamilyName)
            .NotEmpty().WithMessage("Family name is required")
            .MaximumLength(200).WithMessage("Family name must not exceed 200 characters");
    }

    private static bool BeAtLeast18YearsOld(DateTime dateOfBirth)
    {
        return dateOfBirth <= DateTime.UtcNow.AddYears(-18);
    }
}

public class RegisterParentCommandHandler : IRequestHandler<RegisterParentCommand, Result<AuthResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IDataEncryptor _encryptor;

    public RegisterParentCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IDataEncryptor encryptor)
    {
        _context = context;
        _identityService = identityService;
        _encryptor = encryptor;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterParentCommand request, CancellationToken cancellationToken)
    {
        var emailHash = _encryptor.ComputeHash(request.Email.ToLowerInvariant());
        var emailExists = await _context.Users
            .AnyAsync(u => u.NormalizedEmailHash == emailHash, cancellationToken);

        if (emailExists)
        {
            return Error.Conflict("A user with this email already exists");
        }

        var family = FamilyService.Create(request.FamilyName);

        var passwordHash = _identityService.HashPassword(request.Password);

        var user = UserService.CreateParent(
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            family.Id,
            emailHash);

        var mainAccount = AccountService.CreateMain(user.Id);

        var (accessToken, jwtId) = _identityService.GenerateAccessToken(user);

        var refreshToken = RefreshTokenService.Create(
            user.Id,
            jwtId,
            30);

        _context.Families.Add(family);
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
            family.Id,
            accessToken,
            refreshToken.Token);
    }
}
