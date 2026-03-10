using KidBank.Domain.Constants;

namespace KidBank.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);

    public static implicit operator Result(Error error) => new(false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result");

    internal Result(T? value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static implicit operator Result<T>(T value) => new(value, true, null);
    public static implicit operator Result<T>(Error error) => new(default, false, error);
}

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entityName, Guid id) =>
        new(ErrorCodes.NotFound, $"{entityName} with id {id} was not found");

    public static Error NotFound(string message) =>
        new(ErrorCodes.NotFound, message);

    public static Error Validation(string message) =>
        new(ErrorCodes.Validation, message);

    public static Error Unauthorized(string message = "You are not authorized to perform this action") =>
        new(ErrorCodes.Unauthorized, message);

    public static Error Forbidden(string message = "You do not have permission to access this resource") =>
        new(ErrorCodes.Forbidden, message);

    public static Error Conflict(string message) =>
        new(ErrorCodes.Conflict, message);

    public static Error InsufficientFunds() =>
        new(ErrorCodes.InsufficientFunds, "Insufficient funds for this operation");

    public static Error SpendingLimitExceeded(decimal limit, decimal attempted) =>
        new(ErrorCodes.SpendingLimitExceeded, $"Spending limit of {limit} exceeded. Attempted to spend {attempted}");

    public static Error ConcurrencyConflict() =>
        new(ErrorCodes.ConcurrencyConflict, "The record was modified by another user. Please retry.");

    public static Error InvalidOperation(string message) =>
        new(ErrorCodes.InvalidOperation, message);

    public static Error InternalError(string message = "An unexpected error occurred") =>
        new(ErrorCodes.InternalError, message);
}
