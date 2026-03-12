using KidBank.Application.Features.Chat.Commands;
using KidBank.Application.Features.Chat.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class ChatController : BaseApiController
{
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendChatMessageCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Guid? withUserId = null)
    {
        var result = await Mediator.Send(new GetChatHistoryQuery(pageNumber, pageSize, withUserId));
        return HandleResult(result);
    }
}
