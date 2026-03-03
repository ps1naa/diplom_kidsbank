using KidBank.Application.Features.Accounts.Commands;
using KidBank.Application.Features.Accounts.Queries;
using KidBank.Application.Features.Transactions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidBank.API.Controllers;

[Authorize]
public class AccountsController : BaseApiController
{
    [HttpGet("my")]
    public async Task<IActionResult> GetMyAccounts()
    {
        var result = await Mediator.Send(new GetMyAccountsQuery());
        return HandleResult(result);
    }

    [HttpGet("kid/{kidId:guid}")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> GetKidAccounts(Guid kidId)
    {
        var result = await Mediator.Send(new GetKidAccountsQuery(kidId));
        return HandleResult(result);
    }

    [HttpGet("{accountId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(
        Guid accountId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? type = null)
    {
        var result = await Mediator.Send(new GetTransactionsQuery(accountId, pageNumber, pageSize, fromDate, toDate, type));
        return HandleResult(result);
    }

    [HttpPost("savings")]
    public async Task<IActionResult> CreateSavingsAccount([FromBody] CreateSavingsAccountCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResultCreated(result);
    }

    [HttpPost("deposit")]
    [Authorize(Policy = "ParentOnly")]
    public async Task<IActionResult> DepositToKid([FromBody] DepositToKidCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferBetweenAccountsCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("topup")]
    public async Task<IActionResult> TopUp([FromBody] TopUpAccountCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
