using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok();

        return HandleError(result.Error!);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return HandleError(result.Error!);
    }

    protected IActionResult HandleResultCreated<T>(Result<T> result, string? location = null)
    {
        if (result.IsSuccess)
        {
            if (!string.IsNullOrEmpty(location))
                return Created(location, result.Value);
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        return HandleError(result.Error!);
    }

    private IActionResult HandleError(Error error) => error.Code switch
    {
        "NOT_FOUND" => NotFound(new { error.Code, error.Message }),
        "UNAUTHORIZED" => Unauthorized(new { error.Code, error.Message }),
        "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, new { error.Code, error.Message }),
        "VALIDATION_ERROR" => BadRequest(new { error.Code, error.Message }),
        "CONFLICT" => Conflict(new { error.Code, error.Message }),
        "INSUFFICIENT_FUNDS" => BadRequest(new { error.Code, error.Message }),
        "SPENDING_LIMIT_EXCEEDED" => BadRequest(new { error.Code, error.Message }),
        "CONCURRENCY_CONFLICT" => Conflict(new { error.Code, error.Message }),
        "INVALID_OPERATION" => BadRequest(new { error.Code, error.Message }),
        _ => StatusCode(StatusCodes.Status500InternalServerError, new { error.Code, error.Message })
    };
}
