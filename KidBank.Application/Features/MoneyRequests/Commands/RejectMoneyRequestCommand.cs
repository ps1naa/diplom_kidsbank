using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.MoneyRequests.Commands;

public record RejectMoneyRequestCommand(
    Guid RequestId,
    string? Note = null) : IRequest<Result<MoneyRequestDto>>;

public class RejectMoneyRequestCommandValidator : AbstractValidator<RejectMoneyRequestCommand>
{
    public RejectMoneyRequestCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("Request ID is required");
    }
}

public class RejectMoneyRequestCommandHandler : IRequestHandler<RejectMoneyRequestCommand, Result<MoneyRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public RejectMoneyRequestCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MoneyRequestDto>> Handle(RejectMoneyRequestCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can reject money requests");
        }

        var moneyRequest = await _context.MoneyRequests
            .Include(mr => mr.Kid)
            .Include(mr => mr.Parent)
            .FirstOrDefaultAsync(mr => mr.Id == request.RequestId, cancellationToken);

        if (moneyRequest == null)
        {
            return Error.NotFound("Money request", request.RequestId);
        }

        if (moneyRequest.ParentId != _currentUserService.UserId)
        {
            return Error.Forbidden("You can only reject requests directed to you");
        }

        if (moneyRequest.Status != MoneyRequestStatus.Pending)
        {
            return Error.InvalidOperation("Can only reject pending requests");
        }

        MoneyRequestService.Reject(moneyRequest, request.Note);
        await _context.SaveChangesAsync(cancellationToken);

        return new MoneyRequestDto(
            moneyRequest.Id,
            moneyRequest.KidId,
            $"{moneyRequest.Kid.FirstName} {moneyRequest.Kid.LastName}",
            moneyRequest.ParentId,
            $"{moneyRequest.Parent.FirstName} {moneyRequest.Parent.LastName}",
            moneyRequest.Amount,
            moneyRequest.Currency,
            moneyRequest.Reason,
            moneyRequest.Status.ToString(),
            moneyRequest.ResponseNote,
            moneyRequest.CreatedAt,
            moneyRequest.RespondedAt);
    }
}
