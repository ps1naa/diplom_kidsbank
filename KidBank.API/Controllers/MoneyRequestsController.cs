using KidBank.Application.Features.MoneyRequests.Commands;
using KidBank.Application.Features.MoneyRequests.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class MoneyRequestsController : BaseApiController
{
    [HttpPost]
    [Authorize(Policy = "KidOnly")]
    public async Task<IActionResult> CreateMoneyRequest([FromBody] CreateMoneyRequestCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResultCreated(result);
    }

    [HttpGet("my")]
    [Authorize(Policy = "KidOnly")]
    public async Task<IActionResult> GetMyMoneyRequests()
    {
        var result = await Mediator.Send(new GetMyMoneyRequestsQuery());
        return HandleResult(result);
    }

    [HttpGet("pending")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> GetPendingMoneyRequests()
    {
        var result = await Mediator.Send(new GetPendingMoneyRequestsQuery());
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> ApproveMoneyRequest(Guid id, [FromBody] ApproveMoneyRequestRequest? request = null)
    {
        var command = new ApproveMoneyRequestCommand(id, request?.Note);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> RejectMoneyRequest(Guid id, [FromBody] RejectMoneyRequestRequest? request = null)
    {
        var command = new RejectMoneyRequestCommand(id, request?.Note);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public record ApproveMoneyRequestRequest(string? Note);

public record RejectMoneyRequestRequest(string? Note);
