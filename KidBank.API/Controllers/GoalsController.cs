using KidBank.Application.Features.Goals.Commands;
using KidBank.Application.Features.Goals.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class GoalsController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] CreateWishlistGoalCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResultCreated(result);
    }

    [HttpPut("{goalId:guid}")]
    public async Task<IActionResult> UpdateGoal(Guid goalId, [FromBody] UpdateGoalRequest request)
    {
        var command = new UpdateGoalCommand(
            goalId, 
            request.Title, 
            request.TargetAmount, 
            request.Description, 
            request.ImageUrl, 
            request.TargetDate);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{goalId:guid}")]
    public async Task<IActionResult> DeleteGoal(Guid goalId)
    {
        var result = await Mediator.Send(new DeleteGoalCommand(goalId));
        return HandleResult(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyGoals([FromQuery] bool includeCompleted = false)
    {
        var result = await Mediator.Send(new GetMyGoalsQuery(includeCompleted));
        return HandleResult(result);
    }

    [HttpGet("kid/{kidId:guid}")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> GetKidGoals(Guid kidId, [FromQuery] bool includeCompleted = false)
    {
        var result = await Mediator.Send(new GetKidGoalsQuery(kidId, includeCompleted));
        return HandleResult(result);
    }

    [HttpPost("{goalId:guid}/deposit")]
    public async Task<IActionResult> DepositToGoal(Guid goalId, [FromBody] DepositToGoalRequest request)
    {
        var command = new DepositToGoalCommand(goalId, request.Amount);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public record UpdateGoalRequest(
    string Title,
    decimal TargetAmount,
    string? Description = null,
    string? ImageUrl = null,
    DateTime? TargetDate = null);

public record DepositToGoalRequest(decimal Amount);
