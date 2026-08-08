using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Teams.Api.IntegrationTests.TestHelpers;
using Teams.Api.IntegrationTests.TestServices;
using Teams.Core.Services.Invitations;
using Teams.Data.Context;

namespace Teams.Api.IntegrationTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Teams.Api.Program>
{
    // A shared-cache, named in-memory SQLite database - kept alive for this factory's lifetime by holding this
    // connection open. Every ApiDbContext created against this same connection string sees the same data.
    private readonly SqliteConnection _connection = new($"Data Source=file:{Guid.NewGuid():N};Mode=Memory;Cache=Shared");

    public ApiWebApplicationFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Drop the real ApiDbContext (pointed at the configured Reader connection string) and
            // IApiDbContextFactory (which independently opens Reader/Writer connections per call) entirely.
            // Everything in a request - repositories and UnitOfWork alike - shares one Scoped ApiDbContext
            // against the in-memory database instead.
            services
                .RemoveService<ApiDbContext>()
                .RemoveService<DbContextOptions<ApiDbContext>>()
                .RemoveService<IApiDbContextFactory>();

            services.AddDbContext<ApiDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<IApiDbContextFactory, TestApiDbContextFactory>();

            // Replace the invitation sender with a test service
            services.RemoveService<IGameInvitationDispatcher>()
                .AddSingleton<IGameInvitationDispatcher, TestGameInvitationDispatcher>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ApiDbContext>().Database.Migrate();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _connection.Dispose();

        base.Dispose(disposing);
    }
}