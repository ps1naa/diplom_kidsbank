using FluentValidation;
using KidBank.Application.Common.Models;
using MediatR;

namespace KidBank.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
            var error = Error.Validation(errorMessage);

            if (typeof(TResponse).IsGenericType)
            {
                var resultType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) });
                var genericFailure = failureMethod!.MakeGenericMethod(resultType);
                return (TResponse)genericFailure.Invoke(null, new object[] { error })!;
            }

            return (TResponse)(object)Result.Failure(error);
        }

        return await next();
    }
}
