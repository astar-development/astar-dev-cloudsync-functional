using AStar.Dev.CloudSyncFunctional.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AStar.Dev.CloudSyncFunctional.Tests.Integration.TestData;

/// <summary>Provides a shared in-memory SQLite connection for an integration test class.</summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    /// <summary>Gets the shared open SQLite connection.</summary>
    public SqliteConnection Connection { get; } = new("DataSource=:memory:");

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        await Connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(Connection).Options;
        await using var context = new AppDbContext(options);

        // EnsureCreated used here because no EF migrations exist yet (RED commit — stub only).
        // Replace with MigrateAsync once the first migration is generated in the GREEN phase.
        await context.Database.EnsureCreatedAsync();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync() => await Connection.DisposeAsync();
}
