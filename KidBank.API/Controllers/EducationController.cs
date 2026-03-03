using KidBank.Application.Features.Education.Commands;
using KidBank.Application.Features.Education.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class EducationController : BaseApiController
{
    [HttpGet("lessons")]
    public async Task<IActionResult> GetLessons()
    {
        var result = await Mediator.Send(new GetLessonsQuery());
        return HandleResult(result);
    }

    [HttpGet("lessons/{id:guid}")]
    public async Task<IActionResult> GetLessonDetails(Guid id)
    {
        var result = await Mediator.Send(new GetLessonDetailsQuery(id));
        return HandleResult(result);
    }

    [HttpPost("quiz/submit")]
    public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetEducationProgress()
    {
        var result = await Mediator.Send(new GetEducationProgressQuery());
        return HandleResult(result);
    }

    [HttpPost("xp/add")]
    public async Task<IActionResult> AddXp([FromBody] AddXpCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("streak/update")]
    public async Task<IActionResult> UpdateStreak()
    {
        var result = await Mediator.Send(new UpdateStreakCommand());
        return HandleResult(result);
    }

    [HttpPost("achievement/unlock")]
    public async Task<IActionResult> UnlockAchievement([FromBody] UnlockAchievementCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
