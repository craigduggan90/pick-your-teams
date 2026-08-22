using Teams.Api.Infrastructure.Converters;
using Teams.Api.Infrastructure.Errors;
using Teams.Api.Infrastructure.Swagger;
using Teams.Api.Infrastructure.Versioning;
using Teams.Api.Services;
using Teams.Common;
using Teams.Core;
using Teams.Core.Services;
using Teams.Data;

namespace Teams.Api.Infrastructure;

public static class Startup
{
    public static WebApplicationBuilder ConfigureTeamsServices(this WebApplicationBuilder builder)
    {
        builder
            .AddCommonServices()
            .AddCoreServices()
            .AddDataServices()
            .AddVersioning()
            .AddSwaggerDocumentation()
            .AddErrorHandling();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IActorAccessor, ActorAccessor>();

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
            });
        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        return builder;
    }

    public static WebApplication ConfigureTeamsApplication(this WebApplication app)
    {
        app.UseSwaggerDocumentation();
        app.UseErrorHandling();

        app.UseRouting();
        app.MapControllers();
        return app;
    }
}