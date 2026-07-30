using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure.Errors;
using Teams.Api.Infrastructure.Swagger;
using Teams.Api.Infrastructure.Validation;
using Teams.Api.Infrastructure.Versioning;
using Teams.Common;
using Teams.Core;
using Teams.Data;

var builder = WebApplication.CreateBuilder(args);
builder
    .AddCommonServices()
    .AddCoreServices()
    .AddDataServices()
    .AddVersioning()
    .AddRequestValidators()
    .AddSwaggerDocumentation()
    .AddErrorHandling();

builder.Services.AddControllers();
builder.Services.AddRouting();

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