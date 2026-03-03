using KidBank.Application.Features.SpendingLimits.Commands;
using KidBank.Application.Features.SpendingLimits.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize(Policy = "ParentOnly")]
public class SpendingLimitsController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> SetSpendingLimit([FromBody] SetSpendingLimitCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResultCreated(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSpendingLimit(Guid id, [FromBody] UpdateSpendingLimitRequest request)
    {
        var command = new UpdateSpendingLimitCommand(id, request.LimitAmount);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("kid/{kidId:guid}")]
    public async Task<IActionResult> GetSpendingLimits(Guid kidId)
    {
        var result = await Mediator.Send(new GetSpendingLimitsQuery(kidId));
        return HandleResult(result);
    }
}

public record UpdateSpendingLimitRequest(decimal LimitAmount);
