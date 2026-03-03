using KidBank.Application.Features.Families.Commands;
using KidBank.Application.Features.Families.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class FamiliesController : BaseApiController
{
    [HttpPost]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResultCreated(result);
    }

    [HttpPost("invite")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> GenerateInvitation([FromBody] GenerateKidInvitationCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await Mediator.Send(new GetFamilyDashboardQuery());
        return HandleResult(result);
    }

    [HttpGet("kids")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> GetKids()
    {
        var result = await Mediator.Send(new GetKidsQuery());
        return HandleResult(result);
    }
}
