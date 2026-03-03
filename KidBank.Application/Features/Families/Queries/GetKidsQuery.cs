using KidBank.Application.Common.Interfaces;
using KidBank.Application.Common.Models;
using KidBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Features.Families.Queries;

public record GetKidsQuery : IRequest<Result<List<KidDto>>>;

public record KidDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    DateTime DateOfBirth,
    int Age,
    int TotalXp,
    int CurrentStreak,
    DateTime CreatedAt);

public class GetKidsQueryHandler : IRequestHandler<GetKidsQuery, Result<List<KidDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetKidsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<KidDto>>> Handle(GetKidsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsParent)
        {
            return Error.Forbidden("Only parents can view kids list");
        }

        if (!_currentUserService.FamilyId.HasValue)
        {
            return Error.InvalidOperation("User does not belong to a family");
        }

        var kids = await _context.Users
            .Where(u => u.FamilyId == _currentUserService.FamilyId.Value && u.Role == UserRole.Kid && !u.IsDeleted)
            .Select(k => new KidDto(
                k.Id,
                k.Email,
                k.FirstName,
                k.LastName,
                k.AvatarUrl,
                k.DateOfBirth,
                CalculateAge(k.DateOfBirth),
                k.TotalXp,
                k.CurrentStreak,
                k.CreatedAt))
            .ToListAsync(cancellationToken);

        return kids;
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
