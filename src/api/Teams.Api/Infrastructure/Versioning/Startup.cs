using Asp.Versioning;
using System.Diagnostics.CodeAnalysis;
using Teams.Common;

namespace Teams.Api.Infrastructure.Versioning;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static WebApplicationBuilder AddVersioning(this WebApplicationBuilder builder)
    {
        builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader(Constants.ApiVersionHeaderKey));
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return builder;
    }
}