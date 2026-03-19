using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Subscriptions.Commands;

public record CreateSubscriptionCommand(string Plan) : IRequest<Result<SubscriptionDto>>;

public record SubscriptionDto(
    Guid Id,
    Guid FamilyId,
    string Plan,
    decimal PriceAmount,
    string Currency,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    bool AutoRenew,
    DateTime CreatedAt);

public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Plan)
            .Must(p => p is "Monthly" or "Yearly")
            .WithMessage("Plan must be 'Monthly' or 'Yearly'");
    }
}

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public CreateSubscriptionCommandHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken ct)
    {
        if (!_identity.IsParent || !_identity.FamilyId.HasValue)
            return Error.Forbidden("Only parents can manage subscriptions");

        var familyId = _identity.FamilyId.Value;

        var existing = await _context.FamilySubscriptions
            .FirstOrDefaultAsync(s => s.FamilyId == familyId && s.IsActive && s.EndDate > DateTime.UtcNow, ct);

        if (existing != null)
            return Error.Conflict("Family already has an active subscription until " + existing.EndDate.ToString("yyyy-MM-dd"));

        var now = DateTime.UtcNow;
        var (price, endDate) = request.Plan switch
        {
            "Monthly" => (4.99m, now.AddMonths(1)),
            "Yearly" => (39.99m, now.AddYears(1)),
            _ => (4.99m, now.AddMonths(1))
        };

        var sub = new FamilySubscription();
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.Id))!.SetValue(sub, Guid.NewGuid());
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.FamilyId))!.SetValue(sub, familyId);
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.SubscribedByUserId))!.SetValue(sub, _identity.UserId!.Value);
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.Plan))!.SetValue(sub, request.Plan);
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.PriceAmount))!.SetValue(sub, price);
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.Currency))!.SetValue(sub, "BYN");
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.StartDate))!.SetValue(sub, now);
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.EndDate))!.SetValue(sub, endDate);
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.IsActive))!.SetValue(sub, true);
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.AutoRenew))!.SetValue(sub, true);
        typeof(FamilySubscription).GetProperty(nameof(FamilySubscription.CreatedAt))!.SetValue(sub, now);

        _context.FamilySubscriptions.Add(sub);
        await _context.SaveChangesAsync(ct);

        return new SubscriptionDto(sub.Id, sub.FamilyId, sub.Plan, sub.PriceAmount, sub.Currency,
            sub.StartDate, sub.EndDate, sub.IsActive, sub.AutoRenew, sub.CreatedAt);
    }
}
