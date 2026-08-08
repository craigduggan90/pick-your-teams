using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Converters;
using Teams.Api.Infrastructure.Errors;
using Teams.Api.Infrastructure.Swagger;
using Teams.Api.Infrastructure.Versioning;
using Teams.Api.Services;
using Teams.Common;
using Teams.Core;
using Teams.Core.Services;
using Teams.Data;

var builder = WebApplication.CreateBuilder(args);
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

var app = builder.Build();

app.UseSwaggerDocumentation();
app.UseErrorHandling();

app.UseRouting();
app.MapControllers();

app.Run();

namespace Teams.Api
{
    /// <summary>This partial class is required for testing and exclusion from test coverage metrics.</summary>
    [ExcludeFromCodeCoverage]
    public partial class Program;
}