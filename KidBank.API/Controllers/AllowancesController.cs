using KidBank.Application.Features.Allowances.Commands;
using KidBank.Application.Features.Allowances.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize(Policy = "ParentOnly")]
public class AllowancesController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> SetAllowance([FromBody] SetAllowanceCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("kid/{kidId:guid}")]
    public async Task<IActionResult> GetKidAllowance(Guid kidId)
    {
        var result = await Mediator.Send(new GetKidAllowanceQuery(kidId));
        return HandleResult(result);
    }
}
