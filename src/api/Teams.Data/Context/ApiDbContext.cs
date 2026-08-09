using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Teams.Domain.Entities;

namespace Teams.Data.Context;

/// <summary>The database context for the API.</summary>
/// <param name="options">The options for this context.</param>
public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; init; }

    public DbSet<User> Users { get; init; }

    public DbSet<Game> Games { get; init; }

    public DbSet<Invitation> Invitations { get; init; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}