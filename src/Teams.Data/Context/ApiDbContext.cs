using Microsoft.EntityFrameworkCore;
using Teams.Domain.Entities;
using System.Reflection;

namespace Teams.Data.Context;

/// <summary>The database context for the API.</summary>
/// <param name="options">The options for this context.</param>
public class ApiDbContext(DbContextOptions<ApiDbContext> options) : DbContext(options)
{
    /// <summary> The collection of jobs.</summary>
    public DbSet<Job> Jobs { get; init; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}