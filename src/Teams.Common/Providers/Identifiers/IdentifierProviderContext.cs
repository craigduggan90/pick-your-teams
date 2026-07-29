using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Teams.Common.Providers.Identifiers;

/// <summary>Execution context for the <see cref="IdentifierProvider"/> class</summary>
/// <remarks>This is excluded from code coverage as it's behaviour is tested through <see cref="IdentifierProvider" /></remarks>
[ExcludeFromCodeCoverage]
public class IdentifierProviderContext : IDisposable
{
    internal readonly string Value;
    private static readonly ThreadLocal<Stack> ThreadScopeStack = new(() => new Stack());

    /// <summary>Initializes a new instance of the <see cref="IdentifierProviderContext"/> class</summary>
    /// <param name="value">The identifier to fix for the context.</param>
    public IdentifierProviderContext(string value)
    {
        Value = value;
        ThreadScopeStack.Value?.Push(this);
    }

    /// <summary>
    /// The timestamp configured for the current execution context
    /// </summary>
    public static IdentifierProviderContext? Current
    {
        get
        {
            if ((ThreadScopeStack.Value?.Count ?? 0) == 0)
                return null;

            return ThreadScopeStack.Value?.Peek() as IdentifierProviderContext;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ThreadScopeStack.Value?.Pop();
        GC.SuppressFinalize(this);
    }
}