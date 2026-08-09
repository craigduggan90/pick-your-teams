using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Errors.Handlers;

namespace Teams.Api.Infrastructure.Errors;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static IHostApplicationBuilder AddErrorHandling(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddExceptionHandler<MissingHeaderExceptionHandler>()
            .AddExceptionHandler<MissingScopeExceptionHandler>()
            .AddExceptionHandler<NotFoundExceptionHandler>()
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<RequestHandlerExceptionHandler>()
            .AddExceptionHandler<AccessDeniedExceptionHandler>()
            .AddExceptionHandler<UnhandledExceptionHandler>()
            .AddProblemDetails();

        return builder;
    }

    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        => app.UseExceptionHandler();
}