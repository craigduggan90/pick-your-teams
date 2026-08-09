using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Teams.Common.Providers.Temporal;

/// <summary>Execution context for the <see cref="DateTimeOffsetProvider"/> class</summary>
/// <remarks>This is excluded from code coverage as it's behaviour is tested through <see cref="DateTimeOffsetProvider" /></remarks>
[ExcludeFromCodeCoverage]
public class DateTimeOffsetProviderContext : IDisposable
{
    internal DateTimeOffset Timestamp;
    private static readonly ThreadLocal<Stack> ThreadScopeStack = new(() => new Stack());

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeOffsetProviderContext"/> class
    /// </summary>
    /// <param name="timestamp"></param>
    public DateTimeOffsetProviderContext(DateTimeOffset timestamp)
    {
        Timestamp = timestamp;
        ThreadScopeStack.Value?.Push(this);
    }

    /// <summary>
    /// The timestamp configured for the current execution context
    /// </summary>
    public static DateTimeOffsetProviderContext? Current
    {
        get
        {
            if ((ThreadScopeStack.Value?.Count ?? 0) == 0)
                return null;

            return ThreadScopeStack.Value?.Peek() as DateTimeOffsetProviderContext;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ThreadScopeStack.Value?.Pop();
        GC.SuppressFinalize(this);
    }
}