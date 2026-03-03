using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KidBank.Application.Features.Chat.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KidBank.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ISender _mediator;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ISender mediator, ILogger<ChatHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var familyId = GetFamilyId();
        if (familyId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"family_{familyId}");
            _logger.LogInformation("User {UserId} connected to family {FamilyId}", GetUserId(), familyId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var familyId = GetFamilyId();
        if (familyId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"family_{familyId}");
            _logger.LogInformation("User {UserId} disconnected from family {FamilyId}", GetUserId(), familyId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string content, Guid? recipientId = null)
    {
        var command = new SendChatMessageCommand(content, recipientId);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            var message = result.Value;
            var familyId = GetFamilyId();

            if (recipientId.HasValue)
            {
                await Clients.User(recipientId.Value.ToString()).SendAsync("ReceiveMessage", message);
                await Clients.Caller.SendAsync("ReceiveMessage", message);
            }
            else
            {
                await Clients.Group($"family_{familyId}").SendAsync("ReceiveMessage", message);
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Error?.Message ?? "Failed to send message");
        }
    }

    public async Task JoinDirectChat(Guid userId)
    {
        var currentUserId = GetUserId();
        if (currentUserId.HasValue)
        {
            var chatRoomId = GetDirectChatRoomId(currentUserId.Value, userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
        }
    }

    public async Task LeaveDirectChat(Guid userId)
    {
        var currentUserId = GetUserId();
        if (currentUserId.HasValue)
        {
            var chatRoomId = GetDirectChatRoomId(currentUserId.Value, userId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
        }
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                          ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private Guid? GetFamilyId()
    {
        var familyIdClaim = Context.User?.FindFirst("family_id")?.Value;
        return Guid.TryParse(familyIdClaim, out var familyId) ? familyId : null;
    }

    private static string GetDirectChatRoomId(Guid user1, Guid user2)
    {
        var ids = new[] { user1.ToString(), user2.ToString() }.OrderBy(x => x);
        return $"direct_{string.Join("_", ids)}";
    }
}
