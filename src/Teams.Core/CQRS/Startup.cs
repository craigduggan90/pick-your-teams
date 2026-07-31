using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Teams.Core.CQRS.Concrete;

namespace Teams.Core.CQRS;

public static class Startup
{
    public static IHostApplicationBuilder AddMediatorServices(this IHostApplicationBuilder builder)
    {
        var configuration = new MediatorConfiguration();

        // Register the request handlers for the assemblies
        builder.Services.RegisterImplementationsOfType(configuration, typeof(IRequestHandler<>));
        builder.Services.RegisterImplementationsOfType(configuration, typeof(IRequestHandler<,>));

        // Register the mediator service
        builder.Services.AddTransient<IMediator, Mediator>();

        // Return the service collection (for chaining)
        return builder;
    }

    private static IServiceCollection RegisterImplementationsOfType(
        this IServiceCollection services,
        MediatorConfiguration configuration,
        Type genericServiceType)
    {
        // Get all the implementations for the generic request handler service type.
        var handlers = configuration.Assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.ContainsGenericParameters)
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericServiceType))
            .ToList();

        foreach (var implementationType in handlers)
        {
            // Get the interface for the handler (IRequestHandler<> or IRequestHandler<,>).
            var serviceType = implementationType.ImplementedInterfaces
                .Single(i => i.GetGenericTypeDefinition() == genericServiceType);

            // Register the handler as a transient service.
            services.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient));
        }

        return services;
    }
}