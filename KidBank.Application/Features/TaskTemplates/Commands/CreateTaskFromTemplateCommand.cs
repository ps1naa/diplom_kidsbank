using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Application.Features.Tasks.Commands;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.TaskTemplates.Commands;

public record CreateTaskFromTemplateCommand(
    Guid TemplateId,
    Guid AssignedToId,
    DateTime? DueDate = null) : IRequest<Result<TaskDto>>;

public class CreateTaskFromTemplateCommandValidator : AbstractValidator<CreateTaskFromTemplateCommand>
{
    public CreateTaskFromTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.AssignedToId).NotEmpty();
    }
}

public class CreateTaskFromTemplateCommandHandler : IRequestHandler<CreateTaskFromTemplateCommand, Result<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public CreateTaskFromTemplateCommandHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result<TaskDto>> Handle(CreateTaskFromTemplateCommand request, CancellationToken ct)
    {
        if (!_identity.IsParent) return Error.Forbidden("Only parents can create tasks");

        var template = await _context.TaskTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.ParentId == _identity.UserId!.Value, ct);

        if (template == null) return Error.NotFound("TaskTemplate", request.TemplateId);

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.AssignedToId && u.Role == UserRole.Kid && !u.IsDeleted, ct);

        if (kid == null) return Error.NotFound("Kid", request.AssignedToId);
        if (kid.FamilyId != _identity.FamilyId) return Error.Forbidden("Cannot assign tasks to kids from other families");

        var parent = await _context.Users.FirstOrDefaultAsync(u => u.Id == _identity.UserId!.Value, ct);

        var task = TaskService.Create(
            request.AssignedToId, _identity.UserId!.Value,
            template.Title, template.RewardAmount, template.Currency,
            template.Description, request.DueDate);

        _context.TaskAssignments.Add(task);
        await _context.SaveChangesAsync(ct);

        return new TaskDto(task.Id, task.AssignedToId, $"{kid.FirstName} {kid.LastName}",
            task.CreatedById, $"{parent!.FirstName} {parent.LastName}",
            task.Title, task.Description, task.RewardAmount, task.Currency,
            task.DueDate, task.Status.ToString(), task.ProofUrl, task.RejectionReason,
            task.CreatedAt, task.CompletedAt, task.ApprovedAt);
    }
}
