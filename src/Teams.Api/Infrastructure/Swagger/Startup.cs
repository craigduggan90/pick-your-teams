using Asp.Versioning.ApiExplorer;
using Swashbuckle.AspNetCore.Filters;
using Teams.Api.Infrastructure.Swagger.Examples.V1.Common;

namespace Teams.Api.Infrastructure.Swagger;

public static class Startup
{
    public static WebApplicationBuilder AddSwaggerDocumentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.ConfigureOptions<VersionedSwaggerOptions>();
        builder.Services.AddSwaggerGen(options =>
        {
            options.ExampleFilters();
            options.DocInclusionPredicate((docName, apiDesc) =>
                apiDesc.GroupName == docName
                && (apiDesc.ActionDescriptor.AttributeRouteInfo?.Template?.Contains("apiVersion") ?? false));
        });
        builder.Services.AddSwaggerExamplesFromAssemblyOf<CommandValidationProblemDetailsExample>();

        return builder;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"Sol API {description.GroupName}");
            }
        });

        return app;
    }
}