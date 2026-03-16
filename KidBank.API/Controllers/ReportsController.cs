using KidBank.Application.Features.Reports.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize(Policy = "ParentOnly")]
public class ReportsController : BaseApiController
{
    [HttpGet("kid/{kidId:guid}")]
    public async Task<IActionResult> GetKidReport(Guid kidId, [FromQuery] string period = "monthly")
    {
        return HandleResult(await Mediator.Send(new GetKidReportQuery(kidId, period)));
    }
}
