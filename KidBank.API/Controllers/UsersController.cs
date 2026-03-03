using KidBank.Application.Features.Users.Commands;
using KidBank.Application.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await Mediator.Send(new GetCurrentUserQuery());
        return HandleResult(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id));
        return HandleResult(result);
    }
}
