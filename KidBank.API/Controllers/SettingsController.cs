using KidBank.Application.Features.Settings.Commands;
using KidBank.Application.Features.Settings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class SettingsController : BaseApiController
{
    [HttpGet("client")]
    public async Task<IActionResult> GetClientSettings()
    {
        return HandleResult(await Mediator.Send(new GetClientSettingsQuery()));
    }

    [HttpPut("client")]
    public async Task<IActionResult> UpdateClientSetting([FromBody] UpdateClientSettingCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}
