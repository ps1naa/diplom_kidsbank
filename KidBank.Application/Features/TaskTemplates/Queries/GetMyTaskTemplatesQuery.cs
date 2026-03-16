using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.TaskTemplates.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.TaskTemplates.Queries;

public record GetMyTaskTemplatesQuery : IRequest<Result<List<TaskTemplateDto>>>;

public class GetMyTaskTemplatesQueryHandler : IRequestHandler<GetMyTaskTemplatesQuery, Result<List<TaskTemplateDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public GetMyTaskTemplatesQueryHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result<List<TaskTemplateDto>>> Handle(GetMyTaskTemplatesQuery request, CancellationToken ct)
    {
        if (!_identity.IsParent) return Error.Forbidden("Only parents can view task templates");

        var templates = await _context.TaskTemplates
            .Where(t => t.ParentId == _identity.UserId!.Value)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskTemplateDto(t.Id, t.Title, t.Description, t.RewardAmount, t.Currency, t.CreatedAt))
            .ToListAsync(ct);

        return templates;
    }
}
