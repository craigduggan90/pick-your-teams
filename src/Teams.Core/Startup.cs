using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Teams.Core.Services.Players;

namespace Teams.Core;

[ExcludeFromCodeCoverage]
public static class Startup
{
    public static WebApplicationBuilder AddCoreServices(this WebApplicationBuilder builder)
    {

        // builder.Services.AddScoped<IJobsService, JobsService>();
        builder.Services.AddScoped<IPlayersService, PlayersService>();

        return builder;
    }

}