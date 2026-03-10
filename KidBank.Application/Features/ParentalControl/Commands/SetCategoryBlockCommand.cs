using FluentValidation;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.ParentalControl.Commands;

public record SetCategoryBlockCommand(
    Guid KidId,
    Guid CategoryId,
    bool IsBlocked) : IRequest<Result>;

public class SetCategoryBlockCommandValidator : AbstractValidator<SetCategoryBlockCommand>
{
    public SetCategoryBlockCommandValidator()
    {
        RuleFor(x => x.KidId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class SetCategoryBlockCommandHandler : IRequestHandler<SetCategoryBlockCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public SetCategoryBlockCommandHandler(
        IApplicationDbContext context,
        IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(SetCategoryBlockCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
            return Error.Forbidden("Only parents can manage category blocks");

        var kid = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.KidId && 
                                      u.Role == UserRole.Kid && 
                                      u.FamilyId == _currentUserService.FamilyId, 
                                  cancellationToken);

        if (kid == null)
            return Error.NotFound("Kid", request.KidId);

        var category = await _context.SpendingCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (category == null)
            return Error.NotFound("Category", request.CategoryId);

        var existingBlock = await _context.CategoryBlocks
            .FirstOrDefaultAsync(cb => cb.KidId == request.KidId && 
                                       cb.CategoryId == request.CategoryId, 
                                  cancellationToken);

        if (request.IsBlocked && existingBlock == null)
        {
            var block = Domain.Entities.CategoryBlock.Create(request.KidId, request.CategoryId, _currentUserService.UserId!.Value);
            _context.CategoryBlocks.Add(block);
        }
        else if (!request.IsBlocked && existingBlock != null)
        {
            _context.CategoryBlocks.Remove(existingBlock);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
