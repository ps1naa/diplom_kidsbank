using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Queries;

public record GetMissionDetailsQuery(Guid MissionId) : IRequest<Result<MissionDetailsDto>>;

public record MissionDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string? ImageUrl,
    int XpReward,
    List<MissionModuleDto> Modules,
    bool IsCompleted);

public record MissionModuleDto(
    Guid Id,
    string Title,
    string? Description,
    int OrderIndex,
    int XpReward,
    bool IsCompleted,
    int QuizzesCompleted,
    int QuizzesTotal);

public class GetMissionDetailsQueryHandler : IRequestHandler<GetMissionDetailsQuery, Result<MissionDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public GetMissionDetailsQueryHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result<MissionDetailsDto>> Handle(GetMissionDetailsQuery request, CancellationToken ct)
    {
        if (!_identity.UserId.HasValue) return Error.Unauthorized();

        var mission = await _context.EducationMissions
            .Include(m => m.Modules.Where(mod => mod.IsPublished).OrderBy(mod => mod.OrderIndex))
                .ThenInclude(mod => mod.Quizzes)
            .FirstOrDefaultAsync(m => m.Id == request.MissionId && m.IsPublished, ct);

        if (mission == null) return Error.NotFound("Mission", request.MissionId);

        var progressMap = await _context.EducationProgresses
            .Where(p => p.UserId == _identity.UserId.Value && mission.Modules.Select(m => m.Id).Contains(p.ModuleId))
            .ToDictionaryAsync(p => p.ModuleId, ct);

        var modules = mission.Modules.Select(mod =>
        {
            progressMap.TryGetValue(mod.Id, out var progress);
            return new MissionModuleDto(mod.Id, mod.Title, mod.Description, mod.OrderIndex,
                mod.XpReward, progress?.IsCompleted ?? false,
                progress?.QuizzesCompleted ?? 0, mod.Quizzes.Count);
        }).ToList();

        var allCompleted = modules.Count > 0 && modules.All(m => m.IsCompleted);

        return new MissionDetailsDto(mission.Id, mission.Title, mission.Description,
            mission.ImageUrl, mission.XpReward, modules, allCompleted);
    }
}
