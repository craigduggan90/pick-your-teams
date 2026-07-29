using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Teams.Common.Services;
using System.Diagnostics.CodeAnalysis;

namespace Teams.Common;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static WebApplicationBuilder AddCommonServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEnvironmentVariableAccessor, EnvironmentVariableAccessor>();
        return builder;
    }
}