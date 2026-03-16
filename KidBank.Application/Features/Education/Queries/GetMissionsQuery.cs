using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Queries;

public record GetMissionsQuery : IRequest<Result<List<MissionDto>>>;

public record MissionDto(
    Guid Id,
    string Title,
    string? Description,
    string? ImageUrl,
    int OrderIndex,
    int XpReward,
    int MinAge,
    int MaxAge,
    int TotalModules,
    int CompletedModules,
    bool IsCompleted);

public class GetMissionsQueryHandler : IRequestHandler<GetMissionsQuery, Result<List<MissionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public GetMissionsQueryHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result<List<MissionDto>>> Handle(GetMissionsQuery request, CancellationToken ct)
    {
        if (!_identity.UserId.HasValue) return Error.Unauthorized();

        var missions = await _context.EducationMissions
            .Where(m => m.IsPublished)
            .Include(m => m.Modules)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(ct);

        var progressByModule = await _context.EducationProgresses
            .Where(p => p.UserId == _identity.UserId.Value && p.IsCompleted)
            .Select(p => p.ModuleId)
            .ToListAsync(ct);

        var result = missions.Select(m =>
        {
            var totalModules = m.Modules.Count(mod => mod.IsPublished);
            var completedModules = m.Modules.Count(mod => progressByModule.Contains(mod.Id));
            return new MissionDto(m.Id, m.Title, m.Description, m.ImageUrl, m.OrderIndex,
                m.XpReward, m.MinAge, m.MaxAge, totalModules, completedModules,
                totalModules > 0 && completedModules >= totalModules);
        }).ToList();

        return result;
    }
}
