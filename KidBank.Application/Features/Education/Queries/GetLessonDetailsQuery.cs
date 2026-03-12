using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Queries;

public record GetLessonDetailsQuery(Guid LessonId) : IRequest<Result<LessonDetailsDto>>;

public record LessonDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    string Content,
    string? ImageUrl,
    int XpReward,
    List<QuizDto> Quizzes,
    bool IsCompleted,
    int CompletedQuizzes,
    int TotalQuizzes);

public record QuizDto(
    Guid Id,
    string Question,
    List<string> Options,
    int OrderIndex,
    int XpReward);

public class GetLessonDetailsQueryHandler : IRequestHandler<GetLessonDetailsQuery, Result<LessonDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetLessonDetailsQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LessonDetailsDto>> Handle(GetLessonDetailsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var module = await _context.EducationModules
            .Include(m => m.Quizzes.OrderBy(q => q.OrderIndex))
            .FirstOrDefaultAsync(m => m.Id == request.LessonId && m.IsPublished, cancellationToken);

        if (module == null)
        {
            return Error.NotFound("Lesson", request.LessonId);
        }

        var progress = await _context.EducationProgresses
            .FirstOrDefaultAsync(p => 
                p.UserId == _currentUserService.UserId.Value && 
                p.ModuleId == request.LessonId, 
                cancellationToken);

        var quizzes = module.Quizzes.Select(q => new QuizDto(
            q.Id,
            q.Question,
            QuizService.GetOptions(q),
            q.OrderIndex,
            q.XpReward))
            .ToList();

        return new LessonDetailsDto(
            module.Id,
            module.Title,
            module.Description,
            module.Content,
            module.ImageUrl,
            module.XpReward,
            quizzes,
            progress?.IsCompleted ?? false,
            progress?.QuizzesCompleted ?? 0,
            module.Quizzes.Count);
    }
}
