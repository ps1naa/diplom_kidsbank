using KidBank.Domain.Constants;
using KidBank.Domain.Entities;
using KidBank.Domain.Exceptions;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Services;

public static class MoneyRequestService
{
    public static MoneyRequest Create(Guid kidId, Guid parentId, decimal amount, string currency = DefaultValues.DefaultCurrency, string? reason = null)
    {
        if (amount <= 0)
            throw new ArgumentException(ValidationMessages.AmountMustBePositive, nameof(amount));

        return new MoneyRequest
        {
            Id = Guid.NewGuid(),
            KidId = kidId,
            ParentId = parentId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Reason = reason,
            Status = MoneyRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

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
