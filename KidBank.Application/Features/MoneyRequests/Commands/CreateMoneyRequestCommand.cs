using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.MoneyRequests.Commands;

public record CreateMoneyRequestCommand(
    decimal Amount,
    string? Reason = null) : IRequest<Result<MoneyRequestDto>>;

public record MoneyRequestDto(
    Guid Id,
    Guid KidId,
    string KidName,
    Guid ParentId,
    string ParentName,
    decimal Amount,
    string Currency,
    string? Reason,
    string Status,
    string? ResponseNote,
    DateTime CreatedAt,
    DateTime? RespondedAt);

public class CreateMoneyRequestCommandValidator : AbstractValidator<CreateMoneyRequestCommand>
{
    public CreateMoneyRequestCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}

public class CreateMoneyRequestCommandHandler : IRequestHandler<CreateMoneyRequestCommand, Result<MoneyRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public CreateMoneyRequestCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MoneyRequestDto>> Handle(CreateMoneyRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsKid)
        {
            return Error.Forbidden("Only kids can create money requests");
        }

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId!.Value && !u.IsDeleted, cancellationToken);

        if (kid == null)
        {
            return Error.NotFound("User", _currentUserService.UserId!.Value);
        }

        var parent = await _context.Users
            .FirstOrDefaultAsync(u => 
                u.FamilyId == kid.FamilyId && 
                u.Role == UserRole.Parent && 
                !u.IsDeleted, 
                cancellationToken);

        if (parent == null)
        {
            return Error.NotFound("No parent found in family");
        }

        var moneyRequest = MoneyRequestService.Create(
            kid.Id,
            parent.Id,
            request.Amount,
            "RUB",
            request.Reason);

        _context.MoneyRequests.Add(moneyRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return new MoneyRequestDto(
            moneyRequest.Id,
            moneyRequest.KidId,
            $"{kid.FirstName} {kid.LastName}",
            moneyRequest.ParentId,
            $"{parent.FirstName} {parent.LastName}",
            moneyRequest.Amount,
            moneyRequest.Currency,
            moneyRequest.Reason,
            moneyRequest.Status.ToString(),
            moneyRequest.ResponseNote,
            moneyRequest.CreatedAt,
            moneyRequest.RespondedAt);
    }
}
