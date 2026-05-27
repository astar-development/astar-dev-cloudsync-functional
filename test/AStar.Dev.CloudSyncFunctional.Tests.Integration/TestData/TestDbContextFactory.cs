using AStar.Dev.CloudSyncFunctional.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Tests.Integration.TestData;

/// <summary>A minimal <see cref="IDbContextFactory{TContext}"/> implementation for integration tests.</summary>
internal sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> options;

    /// <summary>Initialises a new <see cref="TestDbContextFactory"/> bound to the given connection.</summary>
    /// <param name="connection">The shared SQLite connection to use.</param>
    internal TestDbContextFactory(SqliteConnection connection)
        => options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

    /// <inheritdoc/>
    public AppDbContext CreateDbContext() => new(options);
}
