using KidBank.Domain.Constants;

namespace KidBank.Domain.Exceptions;

public enum ErrorType
{
    NotFound,
    Unauthorized,
    Forbidden,
    Validation,
    Conflict,
    InsufficientFunds,
    SpendingLimitExceeded,
    InvalidOperation,
    ConcurrencyConflict,
    InternalError
}

public class DomainException : Exception
{
    public ErrorType Type { get; }
    public decimal? LimitAmount { get; }
    public decimal? AttemptedAmount { get; }

    private DomainException(ErrorType type, decimal? limitAmount, decimal? attemptedAmount, string message)
        : base(message)
    {
        Type = type;
        LimitAmount = limitAmount;
        AttemptedAmount = attemptedAmount;
    }

    public string Code => Type switch
    {
        ErrorType.NotFound => ErrorCodes.NotFound,
        ErrorType.Unauthorized => ErrorCodes.Unauthorized,
        ErrorType.Forbidden => ErrorCodes.Forbidden,
        ErrorType.Validation => ErrorCodes.Validation,
        ErrorType.Conflict => ErrorCodes.Conflict,
        ErrorType.InsufficientFunds => ErrorCodes.InsufficientFunds,
        ErrorType.SpendingLimitExceeded => ErrorCodes.SpendingLimitExceeded,
        ErrorType.InvalidOperation => ErrorCodes.InvalidOperation,
        ErrorType.ConcurrencyConflict => ErrorCodes.ConcurrencyConflict,
        ErrorType.InternalError => ErrorCodes.InternalError,
        _ => ErrorCodes.InternalError
    };

    public DomainException(ErrorType type, string? message = null)
        : this(type, null, null, message ?? GetDefaultMessage(type))
    {
    }

    public static DomainException NotFound(string entityName, Guid id)
        => new(ErrorType.NotFound, $"{entityName} with id {id} was not found");

    public static DomainException NotFound(string message)
        => new(ErrorType.NotFound, message);

    public static DomainException Unauthorized(string message = "You are not authorized to perform this action")
        => new(ErrorType.Unauthorized, message);

    public static DomainException InsufficientFunds()
        => new(ErrorType.InsufficientFunds, "Insufficient funds for this operation");

    public static DomainException SpendingLimitExceeded(decimal limit, decimal attempted)
        => new(ErrorType.SpendingLimitExceeded, limit, attempted, $"Spending limit of {limit} exceeded. Attempted to spend {attempted}");

    public static DomainException InvalidOperation(string message)
        => new(ErrorType.InvalidOperation, message);

    public static DomainException ConcurrencyConflict()
        => new(ErrorType.ConcurrencyConflict, "The record was modified by another user. Please retry.");

    private static string GetDefaultMessage(ErrorType type) => type switch
    {
        ErrorType.NotFound => "Resource not found",
        ErrorType.Unauthorized => "You are not authorized to perform this action",
        ErrorType.Forbidden => "You do not have permission to access this resource",
        ErrorType.Validation => "Validation failed",
        ErrorType.Conflict => "Conflict occurred",
        ErrorType.InsufficientFunds => "Insufficient funds for this operation",
        ErrorType.SpendingLimitExceeded => "Spending limit exceeded",
        ErrorType.InvalidOperation => "Invalid operation",
        ErrorType.ConcurrencyConflict => "The record was modified by another user. Please retry.",
        _ => "An unexpected error occurred"
    };
}
