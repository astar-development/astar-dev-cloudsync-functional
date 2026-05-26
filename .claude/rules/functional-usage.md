# Functional Type Usage — Result<T,TError> and Option<T>

This is the authoritative rule for how `Result<T,TError>` and `Option<T>` are used across **all layers** of this project. Every other rules file defers to this one on functional-type questions.

## Non-negotiable rules

1. **No raw exceptions cross a layer boundary.** Infrastructure boundaries (Graph API, MSAL, file system, EF Core) catch exceptions and convert them to `Result.Fail`. Service and domain layers never `catch` — they chain `Result`/`Option` operations. The only permitted `try/catch` outside an infrastructure boundary is the `async void` timer callback in `SyncScheduler`.

2. **No null or nullable returns.** Methods never return `T?` when absence is a meaningful state. Use `Option<T>` or `Option<T,TError>` instead.

3. **No `is Ok` / `is Some` / `is Fail` / `is None` pattern matching in production code.** Use `Match` or `MatchAsync` exclusively. This is non-negotiable at every layer including ViewModels.

4. **No intermediate unwrapping.** Never `await` a `Task<Result<T,E>>` into a variable just to call `Match` on the next line. Chain `MatchAsync` directly on the `Task`.

   ```csharp
   // ❌
   var result = await service.GetAsync(ct);
   return result.Match(ok => ok.Value, _ => null);

   // ✅
   return await service.GetAsync(ct)
       .MatchAsync(ok => ok.Value, _ => null);
   ```

5. **Typed error discriminated unions — never `string` as TError.** Every layer boundary defines its own error DU. A `string` error type is a code smell that makes callers unable to branch on failure kind without string parsing.

6. **`throw` is completely banned.** No `throw` statement exists anywhere outside infrastructure `catch` blocks that immediately convert to `Result.Fail(new XxxError(...))`. Every failure is expressed as a `Result`. Infrastructure boundaries are the only place exceptions are caught — not re-thrown.

7. **Silent swallowing is banned everywhere.** The error branch of every `Match`/`MatchAsync` must: (a) log the error using `ILogger<T>` and (b) surface it to the UI via an observable property. An error branch that does neither is a defect.

8. **Expected errors never propagate as exceptions.** If a method can fail, its return type is `Result<T,TError>`. Pass the `Result` up the call chain; callers use `Bind`/`Map`/`Match` to handle it. Unexpected exceptions that escape infrastructure are caught only at the application top-level (see `@.claude/rules/onedrive-di.md`).

9. **`System.IO` is completely banned.** All file system access must go through `IFileSystem` (Testably abstraction). `System.IO.File`, `Directory`, `Path`, `FileInfo`, `DirectoryInfo`, and all other `System.IO` types are forbidden in production code without exception.

10. **Logging is mandatory.** `ILogger<T>` is injected via DI into every service. All error paths log before surfacing or propagating. Use `[LoggerMessage]` source-generated partial methods (see `@.claude/rules/onedrive-background.md`). An error that is neither logged nor surfaced is a defect.

11. **DI is mandatory for services.** Services are never constructed with `new`. All dependencies are resolved through the DI container. See `@.claude/rules/onedrive-di.md` for lifetime guidelines.

## Which type to use — decision guide

| Scenario | Type |
|---|---|
| Operation can fail with a meaningful, typed reason | `Result<T, TError>` |
| Value may or may not be present; absence needs no explanation | `Option<T>` |
| Value may be absent AND absence has a typed reason | `Option<T, TError>` |
| Void operation that can fail (write, delete) | `Result<Unit, TError>` |
| Write operation that cannot meaningfully fail at the domain level | `Task` (only at EF Core repository layer, if EF exceptions are already caught at a higher infrastructure boundary) |

## Layer-by-layer requirements

### Infrastructure layer (Graph API, MSAL, file system)

- All public methods return `Result<T, TError>` or `Option<T>` — never `T?`, never `void`, never raw `Task<T>`.
- `try/catch` is **only** permitted here to convert framework exceptions into `Result.Fail(new XxxError(...))`.
- Each infrastructure interface defines its own error DU (see Error types below).

### Repository / persistence layer

- Single-item lookups: `Task<Option<T>>` — `None` when not found, no need for a typed error.
- Multi-item reads: `Task<IReadOnlyList<T>>` — empty list for no results (not `Option`; emptiness is not absence).
- Writes (upsert, delete, add): `Task<Result<Unit, PersistenceError>>`.
- EF Core exceptions (`DbUpdateException`, `DbUpdateConcurrencyException`) are caught at the repository boundary and converted to `PersistenceError` cases.

### Domain / application service layer

- No `try/catch`.
- Methods that aggregate results from multiple layers return `Result<T, TError>` appropriate to the service's domain (e.g., `SyncError` may wrap `AuthError`, `GraphError`, `PersistenceError`).
- `Bind` is the correct combinator when chaining two `Result`-returning operations where one failure aborts the chain.

### ViewModel layer

- Command handlers (`ReactiveCommand` bodies) receive `Result`/`Option` from services and call `Match`/`MatchAsync` to set observable properties.
- Observable properties are **never** `Option<T>` or `Result<T,E>` — they are plain C# types that ViewModels set inside `Match` lambdas.
- No `is Ok` / `is Some` pattern matching anywhere in a ViewModel — always `Match`.

  ```csharp
  // ❌ ViewModel pattern matching
  var r = await authService.SignInInteractiveAsync(ct);
  if (r is Result<AuthResult, AuthError>.Ok ok) { IsSignedIn = true; ... }

  // ✅ ViewModel using Match
  await authService.SignInInteractiveAsync(ct)
      .MatchAsync(
          ok    => { IsSignedIn = true; SignInStatusText = $"Signed in as {ok.Profile.Email}"; },
          error => { SignInHasError = true; SignInStatusText = error.Message; });
  ```

## Error discriminated union conventions

### Naming

- One error DU per layer boundary: `AuthError`, `GraphError`, `PersistenceError`, `SyncError`.
- Sub-cases are sealed records nested or co-located in the same file.
- All error types are records (immutable).

### Structure template

Every abstract error base declares `string Message { get; }`. Cases with runtime data compute `Message` from their parameters; fixed-message cases override with a constant. Every error DU has a paired static factory class that enforces non-null/non-empty messages, substituting `"unknown error"` when no meaningful text is available.

```csharp
// AuthError
public abstract record AuthError
{
    public abstract string Message { get; }
}
public sealed record AuthCancelledError : AuthError
{
    public override string Message => "Authentication was cancelled.";
}
public sealed record AuthFailedError(string Message) : AuthError;

public static class AuthErrorFactory
{
    public static AuthError Cancelled() => new AuthCancelledError();
    public static AuthError Failed(string? message) => new AuthFailedError(
        string.IsNullOrWhiteSpace(message) ? "Authentication failed: unknown error." : message);
}
```

```csharp
// GraphError
public abstract record GraphError
{
    public abstract string Message { get; }
}
public sealed record GraphNotFoundError(string ItemId) : GraphError
{
    public override string Message => $"Item '{ItemId}' was not found in OneDrive.";
}
public sealed record GraphThrottledError(int RetryAfterSeconds) : GraphError
{
    public override string Message => $"Request throttled. Retry after {RetryAfterSeconds} seconds.";
}
public sealed record GraphUnauthorizedError : GraphError
{
    public override string Message => "Unauthorized. Re-authentication required.";
}
public sealed record GraphNetworkError(string Message) : GraphError;
public sealed record GraphUnexpectedError(string Message) : GraphError;

public static class GraphErrorFactory
{
    public static GraphError NotFound(string itemId) => new GraphNotFoundError(itemId);
    public static GraphError Throttled(int retryAfterSeconds) => new GraphThrottledError(retryAfterSeconds);
    public static GraphError Unauthorized() => new GraphUnauthorizedError();
    public static GraphError Network(string? message) => new GraphNetworkError(
        string.IsNullOrWhiteSpace(message) ? "A network error occurred: unknown error." : message);
    public static GraphError Unexpected(string? message) => new GraphUnexpectedError(
        string.IsNullOrWhiteSpace(message) ? "An unexpected Graph error occurred: unknown error." : message);
}
```

```csharp
// PersistenceError
public abstract record PersistenceError
{
    public abstract string Message { get; }
}
public sealed record ConcurrencyConflictError : PersistenceError
{
    public override string Message => "A concurrency conflict occurred. The record was modified by another operation.";
}
public sealed record ConstraintViolationError(string Message) : PersistenceError;
public sealed record PersistenceUnexpectedError(string Message) : PersistenceError;

public static class PersistenceErrorFactory
{
    public static PersistenceError ConcurrencyConflict() => new ConcurrencyConflictError();
    public static PersistenceError ConstraintViolation(string? detail) => new ConstraintViolationError(
        string.IsNullOrWhiteSpace(detail) ? "A constraint violation occurred: unknown error." : detail);
    public static PersistenceError Unexpected(string? message) => new PersistenceUnexpectedError(
        string.IsNullOrWhiteSpace(message) ? "An unexpected persistence error occurred: unknown error." : message);
}
```

```csharp
// SyncError — wraps lower-layer errors or defines its own cases
public abstract record SyncError
{
    public abstract string Message { get; }
}
public sealed record SyncAuthError(AuthError Inner) : SyncError
{
    public override string Message => Inner.Message;
}
public sealed record SyncGraphError(GraphError Inner) : SyncError
{
    public override string Message => Inner.Message;
}
public sealed record SyncStorageError(PersistenceError Inner) : SyncError
{
    public override string Message => Inner.Message;
}
public sealed record NoFoldersConfiguredError : SyncError
{
    public override string Message => "No folders have been configured for sync.";
}
public sealed record SyncCancelledError : SyncError
{
    public override string Message => "Sync was cancelled.";
}

public static class SyncErrorFactory
{
    public static SyncError AuthFailed(AuthError inner) => new SyncAuthError(inner);
    public static SyncError GraphFailed(GraphError inner) => new SyncGraphError(inner);
    public static SyncError StorageFailed(PersistenceError inner) => new SyncStorageError(inner);
    public static SyncError NoFoldersConfigured() => new NoFoldersConfiguredError();
    public static SyncError Cancelled() => new SyncCancelledError();
}
```

## Pipeline combinators — when to use each

| Combinator | When |
|---|---|
| `Map` | Transform the success value; the transform cannot fail |
| `Bind` | Chain another `Result`-returning operation; failure aborts the chain |
| `Tap` | Side-effect (logging, raising events) without changing the `Result` |
| `Match` / `MatchAsync` | Collapse to a concrete value — use at layer boundaries and in ViewModels |
| `Filter` (Option only) | Discard a present value that fails a predicate |
| `GetOrElse` (Option only) | Extract with a default — use only when absence has a sensible default and needs no branching |

## Option<T> vs Option<T,TError>

| Use | When |
|---|---|
| `Option<T>` | Absence needs no explanation (e.g., repository `GetById` returning `None` = "not found") |
| `Option<T, TError>` | Absence has a typed reason that callers may need to branch on |

`None` in `Option<T>` is self-explanatory at repository level ("record does not exist"). Prefer the simpler form there. Use `Option<T, TError>` in domain services where the reason for absence matters to the caller.

## What is banned everywhere

- `result is Result<T,E>.Ok ok` — banned in all production code
- `option is Option<T>.Some s` — banned in all production code
- `T?` as a return type where `Option<T>` is semantically correct
- `string` as `TError`
- `throw` anywhere — return `Result.Fail(new XxxError(...))` instead
- `try/catch` outside infrastructure boundaries (or the `async void` timer callback)
- Awaiting into an intermediate `result` variable before calling `Match`
- `System.IO.*` types — use `IFileSystem` (Testably abstraction) instead
- Error branch with no log and no UI update (silent swallowing)
- Constructing services with `new` — use DI exclusively
