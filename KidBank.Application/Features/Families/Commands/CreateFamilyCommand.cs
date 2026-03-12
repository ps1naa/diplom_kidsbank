using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using MediatR;

namespace KidBank.Application.Features.Families.Commands;

public record CreateFamilyCommand(string Name) : IRequest<Result<FamilyResponse>>;

public record FamilyResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt);

public class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Family name is required")
            .MaximumLength(200).WithMessage("Family name must not exceed 200 characters");
    }
}

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, Result<FamilyResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public CreateFamilyCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FamilyResponse>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can create families");
        }

        var family = FamilyService.Create(request.Name);

        _context.Families.Add(family);
        await _context.SaveChangesAsync(cancellationToken);

        return new FamilyResponse(family.Id, family.Name, family.CreatedAt);
    }
}
