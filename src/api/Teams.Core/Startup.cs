using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Teams.Core.CQRS;
using Teams.Core.Services.Events;

namespace Teams.Core;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static WebApplicationBuilder AddCoreServices(this WebApplicationBuilder builder)
    {
        builder.AddMediatorServices();
        builder.Services.AddValidatorsFromAssemblyContaining<IMediator>(
            includeInternalTypes: false,
            lifetime: ServiceLifetime.Singleton);

        builder.Services.AddScoped<IEventPublisher, EventPublisher>();

        return builder;
    }

}