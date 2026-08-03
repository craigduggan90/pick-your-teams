using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Teams.Data.Context;

namespace Teams.Data.UnitTests.TestHelpers;

public class TestApiDbContextFactory : IDisposable, IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Filename=:memory:");

    /// <summary>Create a new database context.</summary>
    /// <param name="ensureCreated">Flag indicating whether to ensure that the database is created before returning.</param>
    public ApiDbContext CreateContext(bool ensureCreated = true)
        => CreateContextAsync(ensureCreated, CancellationToken.None).Result;

    /// <summary>Create a new database context.</summary>
    /// <param name="ensureCreated">Flag indicating whether to ensure that the database is created before returning.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task<ApiDbContext> CreateContextAsync(
        bool ensureCreated = true,
        CancellationToken cancellationToken = default)
    {
        await _connection.OpenAsync(cancellationToken);
        var context = new ApiDbContext(new DbContextOptionsBuilder<ApiDbContext>().UseSqlite(_connection).Options);

        if (!ensureCreated)
            return context;

        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
        return context;
    }

    /// <inheritdoc />
    public void Dispose() => _connection.Dispose();

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _connection.DisposeAsync();
}