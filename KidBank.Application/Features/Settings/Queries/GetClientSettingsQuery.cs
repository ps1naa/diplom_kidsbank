using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Settings.Queries;

public record GetClientSettingsQuery : IRequest<Result<Dictionary<string, string>>>;

public class GetClientSettingsQueryHandler : IRequestHandler<GetClientSettingsQuery, Result<Dictionary<string, string>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetClientSettingsQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result<Dictionary<string, string>>> Handle(GetClientSettingsQuery request, CancellationToken cancellationToken)
    {
        if (!_identityService.UserId.HasValue)
            return Error.Unauthorized();

        var settings = await _context.ClientSettings
            .AsNoTracking()
            .Where(s => s.UserId == _identityService.UserId.Value)
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return settings;
    }
}
