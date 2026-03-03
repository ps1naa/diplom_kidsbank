using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Chat.Commands;

public record SendChatMessageCommand(
    string Content,
    Guid? RecipientId = null) : IRequest<Result<ChatMessageDto>>;

public record ChatMessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    Guid? RecipientId,
    string? RecipientName,
    string Content,
    bool IsRead,
    DateTime CreatedAt);

public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters");
    }
}

public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, Result<ChatMessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SendChatMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ChatMessageDto>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue || !_currentUserService.FamilyId.HasValue)
        {
            return Error.Unauthorized();
        }

        var sender = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId.Value, cancellationToken);

        if (sender == null)
        {
            return Error.NotFound("User", _currentUserService.UserId.Value);
        }

        string? recipientName = null;

        if (request.RecipientId.HasValue)
        {
            var recipient = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.RecipientId.Value && !u.IsDeleted, cancellationToken);

            if (recipient == null)
            {
                return Error.NotFound("Recipient", request.RecipientId.Value);
            }

            if (recipient.FamilyId != _currentUserService.FamilyId.Value)
            {
                return Error.Forbidden("Cannot send messages to users outside your family");
            }

            recipientName = $"{recipient.FirstName} {recipient.LastName}";
        }

        ChatMessage message;
        if (request.RecipientId.HasValue)
        {
            message = ChatMessage.CreateDirectMessage(
                _currentUserService.FamilyId.Value,
                _currentUserService.UserId.Value,
                request.RecipientId.Value,
                request.Content);
        }
        else
        {
            message = ChatMessage.CreateFamilyMessage(
                _currentUserService.FamilyId.Value,
                _currentUserService.UserId.Value,
                request.Content);
        }

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return new ChatMessageDto(
            message.Id,
            message.SenderId,
            $"{sender.FirstName} {sender.LastName}",
            message.RecipientId,
            recipientName,
            message.Content,
            message.IsRead,
            message.CreatedAt);
    }
}
