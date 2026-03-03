using KidBank.Application.Features.Tasks.Commands;
using KidBank.Application.Features.Tasks.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class TasksController : BaseApiController
{
    [HttpPost]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResultCreated(result);
    }

    [HttpPut("{taskId:guid}")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateTaskRequest request)
    {
        var command = new UpdateTaskCommand(taskId, request.Title, request.RewardAmount, request.Description, request.DueDate);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{taskId:guid}")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> DeleteTask(Guid taskId)
    {
        var result = await Mediator.Send(new DeleteTaskCommand(taskId));
        return HandleResult(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetAssignedTasks([FromQuery] string? status = null)
    {
        var result = await Mediator.Send(new GetAssignedTasksQuery(status));
        return HandleResult(result);
    }

    [HttpGet("pending-approval")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> GetPendingApprovalTasks()
    {
        var result = await Mediator.Send(new GetPendingApprovalTasksQuery());
        return HandleResult(result);
    }

    [HttpPost("{taskId:guid}/complete")]
    [Authorize(Policy = "KidOnly")]
    public async Task<IActionResult> CompleteTask(Guid taskId, [FromBody] CompleteTaskRequest? request = null)
    {
        var command = new CompleteTaskCommand(taskId, request?.ProofUrl);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{taskId:guid}/approve")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> ApproveTask(Guid taskId)
    {
        var result = await Mediator.Send(new ApproveTaskCommand(taskId));
        return HandleResult(result);
    }

    [HttpPost("{taskId:guid}/reject")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> RejectTask(Guid taskId, [FromBody] RejectTaskRequest? request = null)
    {
        var command = new RejectTaskCommand(taskId, request?.Reason);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public record UpdateTaskRequest(
    string Title,
    decimal RewardAmount,
    string? Description = null,
    DateTime? DueDate = null);

public record CompleteTaskRequest(string? ProofUrl);

public record RejectTaskRequest(string? Reason);
