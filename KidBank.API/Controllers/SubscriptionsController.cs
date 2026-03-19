using KidBank.Application.Features.Subscriptions.Commands;
using KidBank.Application.Features.Subscriptions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class SubscriptionsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetSubscription()
    {
        var result = await Mediator.Send(new GetSubscriptionQuery());
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> Subscribe([FromBody] CreateSubscriptionCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResultCreated(result);
    }

    [HttpPost("cancel")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> Cancel()
    {
        var result = await Mediator.Send(new CancelSubscriptionCommand());
        return HandleResult(result);
    }
}
