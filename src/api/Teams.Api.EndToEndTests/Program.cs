using Teams.Api.EndToEndTests.SeedData;
using Teams.Api.EndToEndTests.TestHelpers;
using Teams.Api.EndToEndTests.TestMiddleware;
using Teams.Api.EndToEndTests.TestServices;
using Teams.Api.Infrastructure;
using Teams.Core.Services.Events;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureTeamsServices();

builder.Services.AddTransient<IDataSeeder, DataSeeder>();

// Swap out services for test services as necessary
builder.Services.RemoveService<IEventPublisher>()
    .AddSingleton<IEventPublisher, TestEventPublisher>();

var app = builder.Build();

app.UseMiddleware<ActorResolverMiddleware>();
app.ConfigureTeamsApplication();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
    await seeder.SetupDatabaseAsync();
    await seeder.SeedDataAsync();
}

app.Run();