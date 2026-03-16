using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Services;
using MediatR;

namespace KidBank.Application.Features.TaskTemplates.Commands;

public record CreateTaskTemplateCommand(
    string Title,
    decimal RewardAmount,
    string? Description = null) : IRequest<Result<TaskTemplateDto>>;

public record TaskTemplateDto(
    Guid Id,
    string Title,
    string? Description,
    decimal RewardAmount,
    string Currency,
    DateTime CreatedAt);

public class CreateTaskTemplateCommandValidator : AbstractValidator<CreateTaskTemplateCommand>
{
    public CreateTaskTemplateCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RewardAmount).GreaterThanOrEqualTo(0);
    }
}

public class CreateTaskTemplateCommandHandler : IRequestHandler<CreateTaskTemplateCommand, Result<TaskTemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identity;

    public CreateTaskTemplateCommandHandler(IApplicationDbContext context, IIdentityService identity)
    {
        _context = context;
        _identity = identity;
    }

    public async Task<Result<TaskTemplateDto>> Handle(CreateTaskTemplateCommand request, CancellationToken ct)
    {
        if (!_identity.IsParent) return Error.Forbidden("Only parents can create task templates");

        var template = TaskTemplateService.Create(
            _identity.UserId!.Value, request.Title, request.RewardAmount, "BYN", request.Description);

        _context.TaskTemplates.Add(template);
        await _context.SaveChangesAsync(ct);

        return new TaskTemplateDto(template.Id, template.Title, template.Description,
            template.RewardAmount, template.Currency, template.CreatedAt);
    }
}
