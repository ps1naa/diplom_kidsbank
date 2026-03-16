using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.TaskTemplates.Commands;

public record DeleteTaskTemplateCommand(Guid TemplateId) : IRequest<Result>;

public class DeleteTaskTemplateCommandHandler : IRequestHandler<DeleteTaskTemplateCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public DeleteTaskTemplateCommandHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result> Handle(DeleteTaskTemplateCommand request, CancellationToken ct)
    {
        if (!_identity.IsParent) return Error.Forbidden("Only parents can delete task templates");

        var template = await _context.TaskTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.ParentId == _identity.UserId!.Value, ct);

        if (template == null) return Error.NotFound("TaskTemplate", request.TemplateId);

        _context.TaskTemplates.Remove(template);
        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
