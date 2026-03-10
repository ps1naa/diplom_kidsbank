using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Families.Commands;

public record GenerateKidInvitationCommand(int ValidForDays = 7) : IRequest<Result<InvitationResponse>>;

public record InvitationResponse(
    string Token,
    DateTime ExpiresAt,
    Guid FamilyId,
    string FamilyName);

public class GenerateKidInvitationCommandValidator : AbstractValidator<GenerateKidInvitationCommand>
{
    public GenerateKidInvitationCommandValidator()
    {
        RuleFor(x => x.ValidForDays)
            .InclusiveBetween(1, 30).WithMessage("Invitation validity must be between 1 and 30 days");
    }
}

public class GenerateKidInvitationCommandHandler : IRequestHandler<GenerateKidInvitationCommand, Result<InvitationResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GenerateKidInvitationCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<InvitationResponse>> Handle(GenerateKidInvitationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can generate invitations");
        }

        if (!_currentUserService.FamilyId.HasValue)
        {
            return Error.InvalidOperation("User does not belong to a family");
        }

        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == _currentUserService.FamilyId.Value, cancellationToken);

        if (family == null)
        {
            return Error.NotFound("Family", _currentUserService.FamilyId.Value);
        }

        var invitation = family.CreateInvitation(TimeSpan.FromDays(request.ValidForDays));
        _context.FamilyInvitations.Add(invitation);

        await _context.SaveChangesAsync(cancellationToken);

        return new InvitationResponse(
            invitation.Token,
            invitation.ExpiresAt,
            family.Id,
            family.Name);
    }
}
