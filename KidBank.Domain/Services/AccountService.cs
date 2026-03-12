using KidBank.Domain.Constants;
using KidBank.Domain.Entities;
using KidBank.Domain.Enums;

namespace KidBank.Domain.Services;

public static class AccountService
{
    public static Account CreateMain(Guid userId, string currency = DefaultValues.DefaultCurrency)
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = DefaultValues.MainAccountName,
            Type = AccountType.Main,
            Balance = 0,
            Currency = currency.ToUpperInvariant(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };
    }

    public static Account CreateSavings(Guid userId, string name, string currency = DefaultValues.DefaultCurrency)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ValidationMessages.AccountNameRequired, nameof(name));

        return new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Type = AccountType.Savings,
            Balance = 0,
            Currency = currency.ToUpperInvariant(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };
    }
}
