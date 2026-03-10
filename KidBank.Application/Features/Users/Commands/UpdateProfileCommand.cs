using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Users.Queries;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Users.Commands;

public record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? AvatarUrl) : IRequest<Result<UserProfileDto>>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters")
            .Must(BeValidUrlOrNull).WithMessage("Avatar URL must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
    }

    private static bool BeValidUrlOrNull(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public UpdateProfileCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.Family)
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User", _currentUserService.UserId.Value);
        }

        UserService.UpdateProfile(user, request.FirstName, request.LastName, request.AvatarUrl);

        await _context.SaveChangesAsync(cancellationToken);

        var level = (user.TotalXp / 1000) + 1;

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.AvatarUrl,
            user.DateOfBirth,
            user.TotalXp,
            user.CurrentStreak,
            level,
            user.FamilyId,
            user.Family.Name,
            user.CreatedAt);
    }
}
