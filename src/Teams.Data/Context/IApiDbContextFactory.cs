using Microsoft.EntityFrameworkCore;

namespace Teams.Data.Context;

/// <summary>Defines a factory for creating <see cref="ApiDbContext"/> instances.</summary>
public interface IApiDbContextFactory : IDbContextFactory<ApiDbContext>
{
    /// <summary>Creates a new <see cref="ApiDbContext"/> instance.</summary>
    /// <param name="contextType">The type of context to create.</param>
    /// <returns>A new context instance.</returns>
    /// <remarks>The caller is responsible for disposing the context; it will not be disposed by any dependency injection container.</remarks>
    ApiDbContext CreateDbContext(ContextType contextType);
}