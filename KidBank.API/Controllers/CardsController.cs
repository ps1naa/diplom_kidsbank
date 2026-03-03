using KidBank.Application.Features.Cards.Commands;
using KidBank.Application.Features.Cards.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class CardsController : BaseApiController
{
    [HttpGet("my")]
    public async Task<IActionResult> GetMyCards()
    {
        var result = await Mediator.Send(new GetMyCardsQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCard([FromBody] CreateVirtualCardCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResultCreated(result);
    }

    [HttpPost("{cardId:guid}/freeze")]
    public async Task<IActionResult> FreezeCard(Guid cardId)
    {
        var result = await Mediator.Send(new FreezeCardCommand(cardId));
        return HandleResult(result);
    }

    [HttpPost("{cardId:guid}/unfreeze")]
    public async Task<IActionResult> UnfreezeCard(Guid cardId)
    {
        var result = await Mediator.Send(new UnfreezeCardCommand(cardId));
        return HandleResult(result);
    }
}
