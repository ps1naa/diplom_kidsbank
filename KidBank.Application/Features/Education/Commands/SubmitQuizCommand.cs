using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Education.Commands;

public record SubmitQuizCommand(
    Guid QuizId,
    int SelectedOptionIndex) : IRequest<Result<QuizResultDto>>;

public record QuizResultDto(
    bool IsCorrect,
    int CorrectOptionIndex,
    string? Explanation,
    int XpEarned,
    int TotalXp,
    bool ModuleCompleted);

public class SubmitQuizCommandValidator : AbstractValidator<SubmitQuizCommand>
{
    public SubmitQuizCommandValidator()
    {
        RuleFor(x => x.QuizId)
            .NotEmpty().WithMessage("Quiz ID is required");

        RuleFor(x => x.SelectedOptionIndex)
            .GreaterThanOrEqualTo(0).WithMessage("Selected option index must be non-negative");
    }
}

public class SubmitQuizCommandHandler : IRequestHandler<SubmitQuizCommand, Result<QuizResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;
    private readonly GamificationService _gamificationService;

    public SubmitQuizCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService,
        GamificationService gamificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _gamificationService = gamificationService;
    }

    public async Task<Result<QuizResultDto>> Handle(SubmitQuizCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Error.Unauthorized();
        }

        var quiz = await _context.Quizzes
            .Include(q => q.Module)
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken);

        if (quiz == null)
        {
            return Error.NotFound("Quiz", request.QuizId);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (user == null)
        {
            return Error.NotFound("User", _currentUserService.UserId.Value);
        }

        var isCorrect = QuizService.IsCorrectAnswer(quiz, request.SelectedOptionIndex);
        var xpEarned = 0;

        if (isCorrect)
        {
            xpEarned = quiz.XpReward;
            _gamificationService.AwardXpForQuizCompletion(user, quiz.XpReward, true);
        }

        var progress = await _context.EducationProgresses
            .FirstOrDefaultAsync(p => 
                p.UserId == _currentUserService.UserId.Value && 
                p.ModuleId == quiz.ModuleId, 
                cancellationToken);

        var moduleCompleted = false;

        if (progress == null)
        {
            var totalQuizzes = await _context.Quizzes
                .CountAsync(q => q.ModuleId == quiz.ModuleId, cancellationToken);

            progress = EducationProgressService.Create(
                _currentUserService.UserId.Value,
                quiz.ModuleId,
                totalQuizzes);

            _context.EducationProgresses.Add(progress);
        }

        if (isCorrect)
        {
            EducationProgressService.RecordQuizCompletion(progress, xpEarned);
            moduleCompleted = progress.IsCompleted;

            if (moduleCompleted)
            {
                _gamificationService.AwardXpForModuleCompletion(user, quiz.Module.XpReward);
                xpEarned += quiz.Module.XpReward;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new QuizResultDto(
            isCorrect,
            quiz.CorrectOptionIndex,
            quiz.Explanation,
            xpEarned,
            user.TotalXp,
            moduleCompleted);
    }
}
