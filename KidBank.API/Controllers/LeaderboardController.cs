using KidBank.Application.Features.Leaderboard.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class LeaderboardController : BaseApiController
{
    [HttpGet("family")]
    public async Task<IActionResult> GetFamilyLeaderboard()
    {
        var result = await Mediator.Send(new GetFamilyLeaderboardQuery());
        return HandleResult(result);
    }
}
