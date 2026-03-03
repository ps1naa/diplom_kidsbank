using FluentValidation;
using KidBank.Application.Common.Behaviors;
using KidBank.Domain.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KidBank.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.AddScoped<LedgerService>();
        services.AddScoped<GamificationService>();
        services.AddScoped<SpendingValidationService>();

        return services;
    }
}
