using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Notifications.Queries;

public record GetNotificationsQuery(int Limit = 50) : IRequest<Result<List<NotificationDto>>>;

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    string? ActionUrl,
    bool IsRead,
    DateTime CreatedAt);

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<List<NotificationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public GetNotificationsQueryHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return Error.Unauthorized();

        var notifications = await _context.Notifications
            .Where(n => n.UserId == _currentUserService.UserId.Value)
            .OrderByDescending(n => n.CreatedAt)
            .Take(request.Limit)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type,
                n.Title,
                n.Message,
                n.ActionUrl,
                n.IsRead,
                n.CreatedAt))
            .ToListAsync(cancellationToken);

        return notifications;
    }
}

public record MarkNotificationReadCommand(Guid NotificationId) : IRequest<Result>;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public MarkNotificationReadCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && 
                                      n.UserId == _currentUserService.UserId, 
                                  cancellationToken);

        if (notification == null)
            return Error.NotFound("Notification", request.NotificationId);

        NotificationService.MarkAsRead(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
