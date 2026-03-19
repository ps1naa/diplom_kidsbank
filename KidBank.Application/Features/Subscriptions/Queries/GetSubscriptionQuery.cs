using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Subscriptions.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Subscriptions.Queries;

public record GetSubscriptionQuery : IRequest<Result<SubscriptionStatusDto>>;

public record SubscriptionStatusDto(
    bool IsPro,
    SubscriptionDto? ActiveSubscription,
    FamilyLimitsDto Limits);

public record FamilyLimitsDto(
    int MaxKids,
    int MaxGoalsPerKid,
    int MaxCardsPerKid,
    int MaxTaskTemplates,
    bool HasSpendingLimits,
    bool HasCategoryBlocks,
    bool HasAdvancedAnalytics,
    bool HasAllEducation,
    int MaxSavingsAccounts);

public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, Result<SubscriptionStatusDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public GetSubscriptionQueryHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result<SubscriptionStatusDto>> Handle(GetSubscriptionQuery request, CancellationToken ct)
    {
        if (!_identity.FamilyId.HasValue)
            return Error.Unauthorized();

        var sub = await _context.FamilySubscriptions
            .Where(s => s.FamilyId == _identity.FamilyId.Value && s.IsActive && s.EndDate > DateTime.UtcNow)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(ct);

        var isPro = sub != null;

        SubscriptionDto? dto = sub != null
            ? new SubscriptionDto(sub.Id, sub.FamilyId, sub.Plan, sub.PriceAmount, sub.Currency,
                sub.StartDate, sub.EndDate, sub.IsActive, sub.AutoRenew, sub.CreatedAt)
            : null;

        var limits = isPro
            ? new FamilyLimitsDto(
                MaxKids: 10,
                MaxGoalsPerKid: 20,
                MaxCardsPerKid: 5,
                MaxTaskTemplates: 50,
                HasSpendingLimits: true,
                HasCategoryBlocks: true,
                HasAdvancedAnalytics: true,
                HasAllEducation: true,
                MaxSavingsAccounts: 5)
            : new FamilyLimitsDto(
                MaxKids: 2,
                MaxGoalsPerKid: 3,
                MaxCardsPerKid: 1,
                MaxTaskTemplates: 5,
                HasSpendingLimits: false,
                HasCategoryBlocks: false,
                HasAdvancedAnalytics: false,
                HasAllEducation: false,
                MaxSavingsAccounts: 1);

        return new SubscriptionStatusDto(isPro, dto, limits);
    }
}
