using KidBank.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IApplicationDbContext _context;

    public SubscriptionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFamilyProAsync(Guid familyId, CancellationToken ct = default)
    {
        return await _context.FamilySubscriptions
            .AnyAsync(s => s.FamilyId == familyId && s.IsActive && s.EndDate > DateTime.UtcNow, ct);
    }

    public async Task<SubscriptionLimits> GetLimitsAsync(Guid familyId, CancellationToken ct = default)
    {
        var isPro = await IsFamilyProAsync(familyId, ct);
        return isPro ? SubscriptionLimits.Pro : SubscriptionLimits.Free;
    }
}
