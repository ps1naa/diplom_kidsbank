using KidBank.Application.Features.Categories.Queries;
using KidBank.Application.Features.ParentalControl.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class CategoriesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var result = await Mediator.Send(new GetCategoriesQuery());
        return HandleResult(result);
    }

    [HttpPost("block")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> SetCategoryBlock([FromBody] SetCategoryBlockCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
