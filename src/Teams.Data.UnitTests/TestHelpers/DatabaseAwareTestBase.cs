using Teams.Data.Context;

namespace Teams.Data.UnitTests.TestHelpers;

public abstract class DatabaseAwareTestBase : IAsyncLifetime, IDisposable
{
    private readonly TestApiDbContextFactory _contextFactory = new();
    private ApiDbContext? _context;

    /// <summary>The database provider for the current test context.</summary>
    protected ApiDbContext Context => _context ??= GetContext();

    /// <summary>Get the database provider for the current test context.</summary>
    /// <param name="ensureCreated">Flag indicating whether to ensure that the database is created before returning.</param>
    private ApiDbContext GetContext(bool ensureCreated = true)
        => GetContextAsync(ensureCreated).Result;

    /// <summary>Get the database provider for the current test context.</summary>
    /// <param name="ensureCreated">Flag indicating whether to ensure that the database is created before returning.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    private async Task<ApiDbContext> GetContextAsync(bool ensureCreated = true, CancellationToken cancellationToken = default)
        => _context ??= await _contextFactory.CreateContextAsync(ensureCreated, cancellationToken);

    /// <summary>Called immediately after the class has been created, before it is used.</summary>
    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    /// <summary>Called when an object is no longer needed. Called just before Dispose() if the class also implements that.</summary>
    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        _contextFactory.Dispose();
        _context?.Dispose();
    }
}