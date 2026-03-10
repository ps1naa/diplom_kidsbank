namespace KidBank.Domain.Constants;

public static class ErrorCodes
{
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Validation = "VALIDATION_ERROR";
    public const string Conflict = "CONFLICT";
    public const string InsufficientFunds = "INSUFFICIENT_FUNDS";
    public const string SpendingLimitExceeded = "SPENDING_LIMIT_EXCEEDED";
    public const string InvalidOperation = "INVALID_OPERATION";
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";
    public const string InternalError = "INTERNAL_ERROR";
}
