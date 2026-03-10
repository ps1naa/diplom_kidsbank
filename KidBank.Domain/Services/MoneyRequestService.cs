using KidBank.Domain.Entities;
using KidBank.Domain.Exceptions;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Services;

public static class MoneyRequestService
{
    public static void Approve(MoneyRequest request, string? note = null)
    {
        if (request.Status != MoneyRequestStatus.Pending)
            throw DomainException.InvalidOperation("Only pending requests can be approved");
        request.Status = MoneyRequestStatus.Approved;
        request.ResponseNote = note;
        request.RespondedAt = DateTime.UtcNow;
    }

    public static void Reject(MoneyRequest request, string? note = null)
    {
        if (request.Status != MoneyRequestStatus.Pending)
            throw DomainException.InvalidOperation("Only pending requests can be rejected");
        request.Status = MoneyRequestStatus.Rejected;
        request.ResponseNote = note;
        request.RespondedAt = DateTime.UtcNow;
    }
}
