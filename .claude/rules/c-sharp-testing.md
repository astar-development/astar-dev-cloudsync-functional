# C# Testing Rules

## Frameworks and packages

- **xUnit v3** — `xunit.v3.mtp-v2`, `OutputType=Exe`, `TestingPlatformDotnetTestSupport=true`
- **Shouldly** — all assertions. `Assert.*` is banned.
- **NSubstitute** — permitted when constructing a real dependency is more effort than the benefit (see below). Not a default — only add the package when actually needed.
- No EF Core in-memory provider — ever (see Integration tests).

## Naming

- Test class: `GivenA<Subject>` — describes the context under test.
- Test method: `when_<condition>_then_<expectation>` — snake_case.
- No XML docs or comments on test classes or methods.

## Structure

- AAA pattern. Three sections separated by blank lines. No `// Arrange` / `// Act` / `// Assert` comments — the blank lines and code make it obvious.
- No logic in test methods. No loops, no conditionals. One scenario per test.
- Use test data builders / factory methods for any non-trivial object construction (see Test data).

## Assertions

Always Shouldly:

```csharp
result.ShouldBeOfType<Ok<AccountEntity, PersistenceError>>();
result.ShouldBe(42);
raisedProperties.ShouldContain(nameof(AccountViewModel.Status));
collection.ShouldBeEmpty();
```

## Result<T,TError> and Option<T> in tests

`is Ok` / `is Some` / `is None` / `is Fail` pattern matching **is permitted in test code**. This is the only place — the production ban does not apply here. Extract the value with a pattern match, then assert with Shouldly:

```csharp
// ✅ test code — pattern matching is fine
var result = await repository.GetByIdAsync(id, ct);
result.ShouldBeOfType<Option<AccountEntity>.Some>();
var some = (Option<AccountEntity>.Some)result;
some.Value.Profile.Email.ShouldBe("test@example.com");

// ✅ or with is pattern
result.ShouldBeAssignableTo<Result<Unit, PersistenceError>.Ok>();
```

Never assert on `string.Contains(error)` — assert on the typed error case:

```csharp
var error = result.ShouldBeOfType<Result<Unit, PersistenceError>.Error>();
error.Value.ShouldBeOfType<ConcurrencyConflictError>();
```

## NSubstitute — when to use

Use NSubstitute when constructing a real dependency requires significant setup that would obscure the test's intent. Prefer real instances or hand-rolled test doubles first.

**Use NSubstitute for:**
- `IAuthService` / `IGraphService` in ViewModel or service unit tests — real implementations require MSAL / network.
- `ILogger<T>` — almost always substituted; constructing real loggers adds noise.
- Any external I/O boundary in a unit test where a real implementation is impractical.

**Do not use NSubstitute for:**
- Repositories — use a real SQLite integration test instead.
- `AppDbContext` — never mock EF Core.
- Simple value objects, records, domain types — construct directly.

```csharp
// ✅ NSubstitute for auth in a ViewModel unit test
var auth = Substitute.For<IAuthService>();
auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
    .Returns(Task.FromResult<Result<AuthResult, AuthError>>(
        new Result<AuthResult, AuthError>.Ok(AuthResultFactory.Success(...))));

var sut = new AddAccountWizardViewModel(auth, Substitute.For<IGraphService>());
```

Keep substitute setup minimal — if the setup is longer than the act/assert, consider whether this is the right test level.

## Unit tests

- Construct types directly. No DI container, no host builder.
- `*.Tests.Unit` project — `IsPackable=false`.
- `TreatWarningsAsErrors=true`.

## Integration tests (persistence)

Integration tests that touch the database live in a `*.Tests.Integration` project.

### Never use EF Core in-memory provider

`UseInMemoryDatabase` does not enforce constraints, value converters, or migrations. It will pass tests that fail against real SQLite. It is banned.

### Real SQLite — `:memory:` mode

Use `Microsoft.Data.Sqlite` with `DataSource=:memory:`. This is real SQLite — all constraints, value converters, and migrations behave identically to production. The database exists only while the connection is open.

### Per-class lifecycle via `IClassFixture<DatabaseFixture>`

Each test class that needs a database inherits from a fixture. The fixture owns the `SqliteConnection` (keeping it open for the class lifetime). Each **test** creates its own `AppDbContext` so the change tracker is clean per-test.

```csharp
// DatabaseFixture.cs — in TestData/ folder
public sealed class DatabaseFixture : IAsyncLifetime
{
    public SqliteConnection Connection { get; } = new("DataSource=:memory:");

    public async Task InitializeAsync()
    {
        await Connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await Connection.DisposeAsync();
}
```

```csharp
// GivenAnAccountRepository.cs
public class GivenAnAccountRepository(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    private AppDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(db.Connection)
            .Options);

    [Fact]
    public async Task when_an_account_is_upserted_then_it_can_be_retrieved()
    {
        var entity = TestAccountFactory.Create();
        await using var writeContext = CreateContext();
        var repo = new AccountRepository(writeContext);

        var result = await repo.UpsertAsync(entity, CancellationToken.None);

        await using var readContext = CreateContext();
        var readRepo = new AccountRepository(readContext);
        var found = await readRepo.GetByIdAsync(entity.Id, CancellationToken.None);

        result.ShouldBeOfType<Result<Unit, PersistenceError>.Ok>();
        found.ShouldBeOfType<Option<AccountEntity>.Some>();
    }
}
```

### AppDbContext construction in tests

Always construct `AppDbContext` directly with `DbContextOptions<AppDbContext>` built from `DbContextOptionsBuilder`. Never use the `AppDbContextDesignTimeFactory` in tests — that is for EF tooling only.

### Migrations in integration tests

Call `context.Database.MigrateAsync()` in `DatabaseFixture.InitializeAsync()`. This mirrors production startup and catches migration bugs. Never use `EnsureCreated` in integration tests.

### Test data — shared factory methods

Place static factory classes in a `TestData/` folder within the integration test project. One class per aggregate root:

```csharp
// TestData/TestAccountFactory.cs
public static class TestAccountFactory
{
    public static AccountEntity Create(string? id = null, string email = "test@example.com") =>
        new()
        {
            Id = new AccountId(id ?? Guid.NewGuid().ToString()),
            Profile = AccountProfileFactory.Create("Test User", email),
            IsActive = true,
            SyncConfig = AccountSyncConfigFactory.Default
        };
}
```

Rules:
- All parameters have sensible defaults — callers only specify what the test cares about.
- Factory methods return fully valid entities that pass all constraints.
- Never duplicate factory logic inline in tests — extract to the shared factory.
- If a factory method grows beyond ~15 lines, consider a builder pattern instead.

## `TreatWarningsAsErrors`

Must be `true` in all test projects. Warnings are bugs.
