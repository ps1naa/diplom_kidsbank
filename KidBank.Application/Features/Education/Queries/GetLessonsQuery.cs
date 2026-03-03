using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Queries;

public record GetLessonsQuery : IRequest<Result<List<LessonDto>>>;

public record LessonDto(
    Guid Id,
    string Title,
    string? Description,
    string? ImageUrl,
    int OrderIndex,
    int XpReward,
    int QuizCount,
    bool IsCompleted,
    decimal ProgressPercentage);

public class GetLessonsQueryHandler : IRequestHandler<GetLessonsQuery, Result<List<LessonDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetLessonsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<LessonDto>>> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
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
            .OrderBy(m => m.OrderIndex)
            .Select(m => new
            {
                Module = m,
                QuizCount = m.Quizzes.Count,
                Progress = m.Progresses.FirstOrDefault(p => p.UserId == _currentUserService.UserId.Value)
            })
            .ToListAsync(cancellationToken);

        var lessons = modules.Select(m => new LessonDto(
            m.Module.Id,
            m.Module.Title,
            m.Module.Description,
            m.Module.ImageUrl,
            m.Module.OrderIndex,
            m.Module.XpReward,
            m.QuizCount,
            m.Progress?.IsCompleted ?? false,
            m.Progress?.GetProgressPercentage() ?? 0))
            .ToList();

        return lessons;
    }
}
