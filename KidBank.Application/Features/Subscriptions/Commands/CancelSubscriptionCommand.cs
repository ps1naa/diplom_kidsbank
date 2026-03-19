using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Subscriptions.Commands;

public record CancelSubscriptionCommand : IRequest<Result>;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public CancelSubscriptionCommandHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken ct)
    {
        if (!_identity.IsParent || !_identity.FamilyId.HasValue)
            return Error.Forbidden("Only parents can manage subscriptions");

        var sub = await _context.FamilySubscriptions
            .FirstOrDefaultAsync(s => s.FamilyId == _identity.FamilyId.Value && s.IsActive, ct);

        if (sub == null)
            return Error.NotFound("No active subscription found");

        typeof(Domain.Entities.FamilySubscription).GetProperty(nameof(Domain.Entities.FamilySubscription.AutoRenew))!.SetValue(sub, false);
        typeof(Domain.Entities.FamilySubscription).GetProperty(nameof(Domain.Entities.FamilySubscription.CancelledAt))!.SetValue(sub, DateTime.UtcNow);

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
