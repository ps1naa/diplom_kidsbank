using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Categories.Queries;

public record GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>;

public record CategoryDto(
    Guid Id,
    string Name,
    string? IconName,
    string? ColorHex,
    bool IsBlocked,
    bool IsAllowedForKids);

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCategoriesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return Error.Unauthorized();

        var categories = await _context.SpendingCategories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var blockedCategoryIds = await _context.Set<Domain.Entities.CategoryBlock>()
            .Where(cb => cb.KidId == _currentUserService.UserId.Value)
            .Select(cb => cb.CategoryId)
            .ToListAsync(cancellationToken);

        var result = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.IconName,
            c.ColorHex,
            blockedCategoryIds.Contains(c.Id),
            c.IsAllowedForKids
        )).ToList();

        return result;
    }
}
