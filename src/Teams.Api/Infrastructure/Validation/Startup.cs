using FluentValidation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Teams.Api.Infrastructure.Validation;

public static class Startup
{
    public static WebApplicationBuilder AddRequestValidators(this WebApplicationBuilder builder)
    {
        // Register all validators as IValidator<T>
        builder.Services.AddValidatorsFromAssemblyContaining<IValidationService>(
            includeInternalTypes: false,
            lifetime: ServiceLifetime.Singleton);

        builder.AddValidatorsAsIValidator(typeof(IValidationService).Assembly);
        builder.Services.AddSingleton<IValidationService, ValidationService>();

        return builder;
    }

    /// <summary>
    /// Registers every concrete <see cref="AbstractValidator{T}"/> in the given assembly against
    /// both its closed IValidator&lt;T&gt; and the non-generic IValidator — the latter being what
    /// <see cref="IValidationService"/> resolves as IEnumerable&lt;IValidator&gt; to build its dispatch table.
    /// </summary>
    private static WebApplicationBuilder AddValidatorsAsIValidator(
        this WebApplicationBuilder builder,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        foreach (var type in assembly.GetTypes()
                     .Where(type => type is { IsAbstract: false, IsInterface: false })
                     .Where(type => typeof(IValidator).IsAssignableFrom(type)))
        {
            builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(IValidator), type, lifetime));

            foreach (var closedInterface in type.GetInterfaces()
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            {
                builder.Services.TryAddEnumerable(new ServiceDescriptor(closedInterface, type, lifetime));
            }
        }

        return builder;
    }
}