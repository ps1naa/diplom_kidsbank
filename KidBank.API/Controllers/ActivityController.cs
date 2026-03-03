using KidBank.Application.Features.ActivityFeed.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class ActivityController : BaseApiController
{
    [HttpGet("feed")]
    public async Task<IActionResult> GetActivityFeed([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetActivityFeedQuery(pageNumber, pageSize));
        return HandleResult(result);
    }
}
