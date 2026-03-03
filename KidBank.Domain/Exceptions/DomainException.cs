namespace KidBank.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class InsufficientFundsException : DomainException
{
    public InsufficientFundsException() 
        : base("INSUFFICIENT_FUNDS", "Insufficient funds for this operation") { }
}

public class InvalidOperationDomainException : DomainException
{
    public InvalidOperationDomainException(string message) 
        : base("INVALID_OPERATION", message) { }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, Guid id) 
        : base("NOT_FOUND", $"{entityName} with id {id} was not found") { }
}

public class UnauthorizedDomainException : DomainException
{
    public UnauthorizedDomainException(string message = "You are not authorized to perform this action") 
        : base("UNAUTHORIZED", message) { }
}

public class SpendingLimitExceededException : DomainException
{
    public SpendingLimitExceededException(decimal limit, decimal attempted) 
        : base("SPENDING_LIMIT_EXCEEDED", $"Spending limit of {limit} exceeded. Attempted to spend {attempted}") { }
}

public class ConcurrencyException : DomainException
{
    public ConcurrencyException() 
        : base("CONCURRENCY_CONFLICT", "The record was modified by another user. Please retry.") { }
}
