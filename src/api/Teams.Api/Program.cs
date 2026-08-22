using System.Diagnostics.CodeAnalysis;
using Teams.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args).ConfigureTeamsServices();
var app = builder.Build().ConfigureTeamsApplication();

app.Run();

namespace Teams.Api
{
    /// <summary>This partial class is required for testing and exclusion from test coverage metrics.</summary>
    [ExcludeFromCodeCoverage]
    public partial class Program;
}