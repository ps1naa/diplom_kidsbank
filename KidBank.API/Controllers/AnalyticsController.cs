using KidBank.Application.Features.Analytics.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize(Policy = "ParentOnly")]
public class AnalyticsController : BaseApiController
{
    [HttpGet("kid/{kidId:guid}/summary")]
    public async Task<IActionResult> GetKidSpendingSummary(Guid kidId)
    {
        var result = await Mediator.Send(new GetKidSpendingSummaryQuery(kidId));
        return HandleResult(result);
    }

    [HttpGet("kid/{kidId:guid}/monthly")]
    public async Task<IActionResult> GetMonthlyStats(Guid kidId, [FromQuery] int months = 6)
    {
        var result = await Mediator.Send(new GetMonthlyStatsQuery(kidId, months));
        return HandleResult(result);
    }

    [HttpGet("family/overview")]
    public async Task<IActionResult> GetFamilyAnalytics()
    {
        var result = await Mediator.Send(new GetFamilyAnalyticsQuery());
        return HandleResult(result);
    }
}
