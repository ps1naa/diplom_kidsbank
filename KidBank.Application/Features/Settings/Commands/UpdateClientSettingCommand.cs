using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Settings.Commands;

public record UpdateClientSettingCommand(string Key, string Value) : IRequest<Result>;

public class UpdateClientSettingCommandHandler : IRequestHandler<UpdateClientSettingCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public UpdateClientSettingCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result> Handle(UpdateClientSettingCommand request, CancellationToken cancellationToken)
    {
        if (!_identityService.UserId.HasValue)
            return Error.Unauthorized();

        var userId = _identityService.UserId.Value;

        var existing = await _context.ClientSettings
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Key == request.Key, cancellationToken);

        if (existing != null)
        {
            ClientSettingService.Update(existing, request.Value);
        }
        else
        {
            var setting = ClientSettingService.Create(userId, request.Key, request.Value);
            _context.ClientSettings.Add(setting);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
