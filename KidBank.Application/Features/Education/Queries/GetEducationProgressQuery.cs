using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Queries;

public record GetEducationProgressQuery : IRequest<Result<EducationProgressSummaryDto>>;

public record EducationProgressSummaryDto(
    int TotalModules,
    int CompletedModules,
    int TotalXpEarned,
    int CurrentStreak,
    int Level,
    int XpToNextLevel,
    List<ModuleProgressDto> Modules);

public record ModuleProgressDto(
    Guid ModuleId,
    string ModuleTitle,
    bool IsCompleted,
    int CompletedQuizzes,
    int TotalQuizzes,
    decimal ProgressPercentage,
    int XpEarned);

public class GetEducationProgressQueryHandler : IRequestHandler<GetEducationProgressQuery, Result<EducationProgressSummaryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetEducationProgressQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<EducationProgressSummaryDto>> Handle(GetEducationProgressQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User", _currentUserService.UserId.Value);
        }

        var age = (DateTime.UtcNow - user.DateOfBirth).Days / 365;

        var modules = await _context.EducationModules
            .Where(m => m.IsPublished && m.MinAge <= age && m.MaxAge >= age)
            .Include(m => m.Quizzes)
            .ToListAsync(cancellationToken);

        var progresses = await _context.EducationProgresses
            .Where(p => p.UserId == _currentUserService.UserId.Value)
            .ToListAsync(cancellationToken);

        var moduleProgresses = modules.Select(m =>
        {
            var progress = progresses.FirstOrDefault(p => p.ModuleId == m.Id);
            return new ModuleProgressDto(
                m.Id,
                m.Title,
                progress?.IsCompleted ?? false,
                progress?.QuizzesCompleted ?? 0,
                m.Quizzes.Count,
                progress != null ? EducationProgressService.GetProgressPercentage(progress) : 0,
                progress?.TotalXpEarned ?? 0);
        }).ToList();

        var level = (user.TotalXp / 1000) + 1;
        var xpToNextLevel = (level * 1000) - user.TotalXp;

        return new EducationProgressSummaryDto(
            modules.Count,
            moduleProgresses.Count(p => p.IsCompleted),
            progresses.Sum(p => p.TotalXpEarned),
            user.CurrentStreak,
            level,
            xpToNextLevel,
            moduleProgresses);
    }
}
