using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Teams.Api.Infrastructure.Swagger;

public class VersionedSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Team Picker API",
                Version = description.ApiVersion.ToString(),
                Description = "A facade API for enqueuing and tracking long-running jobs."
                              + (description.IsDeprecated ? " This version is deprecated." : string.Empty)
            });
        }
    }

    public void Configure(string? name, SwaggerGenOptions options) => Configure(options);
}