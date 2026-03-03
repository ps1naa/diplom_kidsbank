using KidBank.Application.Features.Achievements.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class AchievementsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAllAchievements()
    {
        var result = await Mediator.Send(new GetAchievementsQuery());
        return HandleResult(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAchievements()
    {
        var result = await Mediator.Send(new GetMyAchievementsQuery());
        return HandleResult(result);
    }
}
