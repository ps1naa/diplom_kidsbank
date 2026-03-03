using KidBank.Application.Features.Notifications.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class NotificationsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int limit = 50)
    {
        var result = await Mediator.Send(new GetNotificationsQuery(limit));
        return HandleResult(result);
    }

    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var result = await Mediator.Send(new MarkNotificationReadCommand(notificationId));
        return HandleResult(result);
    }
}
