# OneDrive Persistence — EF Core + SQLite

Persistence uses EF Core with the SQLite provider. One `AppDbContext` owns all tables. Each entity has a dedicated `IEntityTypeConfiguration<T>` class in a `Data/Configuration/` folder.

## Packages required

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" />
```

## AppDbContext

```csharp
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AccountEntity>                    Accounts               => Set<AccountEntity>();
    public DbSet<SyncConflictEntity>               SyncConflicts          => Set<SyncConflictEntity>();
    public DbSet<SyncJobEntity>                    SyncJobs               => Set<SyncJobEntity>();
    public DbSet<DriveStateEntity>                 DriveStates            => Set<DriveStateEntity>();
    public DbSet<SyncRuleEntity>                   SyncRules              => Set<SyncRuleEntity>();
    public DbSet<SyncedItemEntity>                 SyncedItems            => Set<SyncedItemEntity>();
    public DbSet<SyncedItemClassificationEntity>   SyncedItemClassifications => Set<SyncedItemClassificationEntity>();
    public DbSet<FileClassificationRuleEntity>     FileClassificationRules => Set<FileClassificationRuleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSqliteFriendlyConversions();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

Always call `UseSqliteFriendlyConversions()` before `ApplyConfigurationsFromAssembly`. It registers value converters for common .NET types (DateTimeOffset, Guid) that SQLite cannot store natively.

## Migrations

- Create migrations with: `dotnet ef migrations add <Name> --project <data-project>`
- Apply on startup: call `context.Database.MigrateAsync(ct)` in the application initializer, not `EnsureCreated`.
- Never edit generated migration files after creation.
- Never create a migration that drops existing data without an explicit user decision.

## Strongly-typed ID mapping

Every strongly-typed ID (e.g. `AccountId`, `OneDriveItemId`, `DriveId`) must be mapped via a value converter in its entity configuration:

```csharp
builder.Property(e => e.Id)
       .HasConversion(id => id.Id, str => new AccountId(str));
```

- The backing type (typically `string` or `Guid`) is what SQLite stores.
- Never store a raw `string` for a domain ID in an entity — always use the strongly-typed wrapper.

## `Option<T>` mapping

`Option<DateTimeOffset>` maps to a nullable ticks column:

```csharp
builder.Property(e => e.LastSyncedAt)
       .HasConversion(SqliteTypeConverters.OptionDateTimeOffsetToNullableTicks)
       .IsRequired(false);
```

`Option<string>` maps to a nullable `TEXT` column:

```csharp
builder.Property(e => e.SomeOptionalText)
       .HasConversion(SqliteTypeConverters.OptionStringToNullableString)
       .IsRequired(false);
```

- `Option.None<T>()` → `NULL` in the database.
- `Option.Some(value)` → the underlying value.
- Never store `Option<T>` using the default EF serialization — always apply an explicit converter.

## ComplexProperty for value objects

For owned value objects with no independent identity (e.g. `AccountProfile`, `StorageQuota`, `AccountSyncConfig`), use `ComplexProperty` rather than a separate table:

### `AccountSyncConfig` shape

```csharp
public sealed class AccountSyncConfig
{
    public LocalSyncPath LocalSyncPath { get; set; }
    public int WorkerCount { get; set; } = 8;  // 1–10; controls parallel job execution
}
```

`WorkerCount` is user-configurable (1–10). Default is 8. Enforce the range in the settings ViewModel — the entity stores the raw int.

```csharp
builder.ComplexProperty(e => e.Profile, p =>
{
    _ = p.Property(prof => prof.DisplayName).HasColumnName("DisplayName");
    _ = p.Property(prof => prof.Email).HasColumnName("Email");
});
```

## Cascade deletes

When an `AccountEntity` is deleted, all dependent entities must also be deleted. Configure `OnDelete(DeleteBehavior.Cascade)` for every relationship that hangs off `AccountEntity`:

- `SyncConflictEntity` (FK: `AccountId`)
- `SyncJobEntity` (FK: `AccountId`)
- `DriveStateEntity` (FK: `AccountId`)
- `SyncRuleEntity` (FK: `AccountId`)
- `SyncedItemEntity` (FK: `AccountId`)

## Repository pattern

Each entity type has a matching repository interface and implementation:

```
IAccountRepository      / AccountRepository
ISyncRuleRepository     / SyncRuleRepository
ISyncedItemRepository   / SyncedItemRepository
IDriveStateRepository   / DriveStateRepository
ISyncRepository         / SyncRepository          (conflicts + jobs)
IFileClassificationRuleRepository / FileClassificationRuleRepository
```

- Single-item reads (`GetByIdAsync`): `Task<Option<T>>` — `None` when not found, no error type needed.
- Multi-item reads: `Task<IReadOnlyList<T>>` — empty list for no results (not `Option`).
- Write operations (`UpsertAsync`, `AddAsync`, `DeleteAsync`): `Task<Result<Unit, PersistenceError>>` — catch `DbUpdateException` / `DbUpdateConcurrencyException` at the repository boundary and convert to `PersistenceError` cases. Never let EF exceptions propagate to callers.
- Repositories are registered as `Transient` — see `@.claude/rules/onedrive-di.md`.
- See `@.claude/rules/functional-usage.md` for the `PersistenceError` DU definition and full rules.

## Upsert pattern

All write methods return `Result<Unit, PersistenceError>`. Catch EF exceptions at the repository:

```csharp
public async Task<Result<Unit, PersistenceError>> UpsertAsync(AccountEntity entity, CancellationToken ct)
{
    try
    {
        var existing = await context.Accounts.FindAsync([entity.Id], ct);
        if (existing is null)
            context.Accounts.Add(entity);
        else
            context.Entry(existing).CurrentValues.SetValues(entity);
        await context.SaveChangesAsync(ct);

        return new Result<Unit, PersistenceError>.Ok(Unit.Default);
    }
    catch (DbUpdateConcurrencyException)
    {
        return new Result<Unit, PersistenceError>.Error(new ConcurrencyConflictError());
    }
    catch (DbUpdateException ex)
    {
        return new Result<Unit, PersistenceError>.Error(new PersistenceUnexpectedError(ex.Message));
    }
}
```

## Entity vs domain model

Entities (`*Entity` classes) are persistence models — mutable classes, EF-configured.  
Domain models (`OneDriveAccount`, `DeltaItem`, etc.) are immutable records used in the application layer.  
Mapping between entity and domain belongs in the service layer, not inside repositories.

## Database location

Store the SQLite database file in the platform-appropriate user config directory (same as the token cache):

| Platform | Path |
|---|---|
| Linux | `~/.config/<app-name>/sync.db` |
| Windows | `%AppData%\<app-name>\sync.db` |
| macOS | `~/Library/Application Support/<app-name>/sync.db` |

Use `IFileSystem` (Testably abstraction) for path construction in production code and tests.
