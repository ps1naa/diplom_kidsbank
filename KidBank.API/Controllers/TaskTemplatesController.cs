using KidBank.Application.Features.TaskTemplates.Commands;
using KidBank.Application.Features.TaskTemplates.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize(Policy = "ParentOnly")]
public class TaskTemplatesController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskTemplateCommand command)
    {
        return HandleResultCreated(await Mediator.Send(command));
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTemplates()
    {
        return HandleResult(await Mediator.Send(new GetMyTaskTemplatesQuery()));
    }

    [HttpDelete("{templateId:guid}")]
    public async Task<IActionResult> Delete(Guid templateId)
    {
        return HandleResult(await Mediator.Send(new DeleteTaskTemplateCommand(templateId)));
    }

    [HttpPost("{templateId:guid}/assign")]
    public async Task<IActionResult> CreateFromTemplate(Guid templateId, [FromBody] AssignFromTemplateRequest request)
    {
        var command = new CreateTaskFromTemplateCommand(templateId, request.AssignedToId, request.DueDate);
        return HandleResultCreated(await Mediator.Send(command));
    }
}

public record AssignFromTemplateRequest(Guid AssignedToId, DateTime? DueDate = null);
