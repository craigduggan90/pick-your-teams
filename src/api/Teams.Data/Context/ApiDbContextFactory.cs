using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Teams.Common.Extensions;

namespace Teams.Data.Context;

/// <summary>A factory for creating <see cref="ApiDbContext"/> instances.</summary>
/// <param name="configuration">The application configuration provider.</param>
public class ApiDbContextFactory(IConfiguration configuration) : IApiDbContextFactory
{
    /// <summary>Creates a new <see cref="ApiDbContext"/> instance.</summary>
    /// <returns>A new context instance.</returns>
    /// <remarks>The caller is responsible for disposing the context; it will not be disposed by any dependency injection container.</remarks>
    public ApiDbContext CreateDbContext()
        => CreateDbContext(ContextType.ReadOnly);

    /// <inheritdoc />
    public ApiDbContext CreateDbContext(ContextType contextType)
    {
        // Get the connection string for the current context type
        var connectionStringKey = contextType == ContextType.ReadWrite ? "Writer" : "Reader";
        var connectionString = configuration.GetConnectionString(connectionStringKey)
                               ?? throw new ArgumentException($"Configuration string not available for {contextType.GetName()} contexts.", nameof(contextType));

        var options = new DbContextOptionsBuilder<ApiDbContext>().UseSqlite(connectionString).Options;

        return new ApiDbContext(options);
    }
}