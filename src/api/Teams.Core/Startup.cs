using Beans.Requestable;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Teams.Core.Services.Events;

namespace Teams.Core;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static WebApplicationBuilder AddCoreServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRequestableServices(options => options.Assemblies = [typeof(Startup).Assembly]);
        builder.Services.AddValidatorsFromAssembly(typeof(Startup).Assembly,
            includeInternalTypes: false,
            lifetime: ServiceLifetime.Singleton);

        builder.Services.AddScoped<IEventPublisher, EventPublisher>();

        return builder;
    }

}