using System.Reflection;
using System.Text.RegularExpressions;
using MediatR;

namespace KidBank.Application.Common.Behaviors;

public partial class SanitizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        SanitizeStringProperties(request);
        return next();
    }

    private static void SanitizeStringProperties(TRequest request)
    {
        var props = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

        foreach (var prop in props)
        {
            var value = (string?)prop.GetValue(request);
            if (value == null) continue;
            prop.SetValue(request, Sanitize(value));
        }

        if (typeof(TRequest).IsClass && !typeof(TRequest).IsValueType)
        {
            SanitizeRecordStringParams(request);
        }
    }

    private static void SanitizeRecordStringParams(TRequest request)
    {
        var fields = typeof(TRequest).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(string));

        foreach (var field in fields)
        {
            var value = (string?)field.GetValue(request);
            if (value == null) continue;
            field.SetValue(request, Sanitize(value));
        }

        var nullableStringFields = typeof(TRequest).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(string));

        foreach (var field in nullableStringFields)
        {
            var value = (string?)field.GetValue(request);
            if (value == null) continue;
            field.SetValue(request, Sanitize(value));
        }
    }

    internal static string Sanitize(string input)
    {
        var result = HtmlTagRegex().Replace(input, string.Empty);

        result = result
            .Replace("javascript:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("vbscript:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("data:", string.Empty, StringComparison.OrdinalIgnoreCase);

        result = EventHandlerRegex().Replace(result, string.Empty);

        return result.Trim();
    }

    [GeneratedRegex(@"<[^>]*>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"on\w+\s*=\s*[""'][^""']*[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerRegex();
}
