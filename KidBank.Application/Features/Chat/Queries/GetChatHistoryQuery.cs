using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Chat.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Chat.Queries;

public record GetChatHistoryQuery(
    int PageNumber = 1,
    int PageSize = 50,
    Guid? WithUserId = null) : IRequest<Result<PaginatedList<ChatMessageDto>>>;

public class GetChatHistoryQueryHandler : IRequestHandler<GetChatHistoryQuery, Result<PaginatedList<ChatMessageDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetChatHistoryQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaginatedList<ChatMessageDto>>> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue || !_currentUserService.FamilyId.HasValue)
        {
            return Error.Unauthorized();
        }

        var query = _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .Where(m => m.FamilyId == _currentUserService.FamilyId.Value);

        if (request.WithUserId.HasValue)
        {
            query = query.Where(m =>
                (m.SenderId == _currentUserService.UserId.Value && m.RecipientId == request.WithUserId.Value) ||
                (m.SenderId == request.WithUserId.Value && m.RecipientId == _currentUserService.UserId.Value));
        }
        else
        {
            query = query.Where(m =>
                m.RecipientId == null ||
                m.SenderId == _currentUserService.UserId.Value ||
                m.RecipientId == _currentUserService.UserId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new ChatMessageDto(
                m.Id,
                m.SenderId,
                m.Sender.FirstName + " " + m.Sender.LastName,
                m.RecipientId,
                m.Recipient != null ? m.Recipient.FirstName + " " + m.Recipient.LastName : null,
                m.Content,
                m.IsRead,
                m.CreatedAt))
            .ToListAsync(cancellationToken);

        messages.Reverse();

        return new PaginatedList<ChatMessageDto>(messages, totalCount, request.PageNumber, request.PageSize);
    }
}
