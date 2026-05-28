---
paths:
    - "**/*.cs"
---

Coding standards and style guidelines / preferences for C# files in this repository that AI must follow.

## Language Version

- Always use the **latest available C# language features** (currently C# 14). Never fall back to older patterns when a newer idiom is cleaner.
- Key C# 14+ idioms to prefer:
  - `field` keyword for semi-auto properties — eliminates explicit backing fields for `RaiseAndSetIfChanged` reactive props:
    ```csharp
    // ✅ C# 14
    public string Name { get; set => this.RaiseAndSetIfChanged(ref field, value); } = string.Empty;
    // ❌ older
    private string name = string.Empty;
    public string Name { get => name; set => this.RaiseAndSetIfChanged(ref name, value); }
    ```
  - Collection expressions `[]` over `new List<T>()` / `new T[]{}` / `Array.Empty<T>()`
  - Primary constructors for simple DI / immutable types
  - Pattern matching (`is`, `switch` expressions) over type-checking `if`/`else` chains

## Naming Conventions

- **Public members**: PascalCase (e.g., `MyClass`, `MyMethod`, `MyProperty`)
- **Private members**: camelCase (e.g., `myVariable`, `myMethod`)
- **Private fields**: camelCase without underscore prefix (e.g., `fieldName`)
- **Constants**: PascalCase
- Use meaningful names that clearly convey purpose; avoid abbreviations unless widely understood
- Use `nameof()` for parameter names in exceptions and logging
- NEVER use single-letter variable names except for loop indices (e.g., `i`, `j`, `k`).

## Namespaces

- Use file-scoped namespaces.
- Namespace names should follow the pattern: Company.Project.Module (e.g., Contoso.Sales.Reporting).
- Use `using` aliases to eliminate noise from verbose third-party type names:
  ```csharp
  // ✅ alias at the top of the file — use the alias throughout
  using GraphClient = Microsoft.Graph.GraphServiceClient;

  // ❌ repeated fully-qualified name in every method signature
  private static Task<...> FooAsync(Microsoft.Graph.GraphServiceClient client, ...) { }
  ```

## Folder Structure

Organise files by **feature / domain concept**, not by type category.

```
// ✅ feature grouping
src/
  Accounts/
    AccountViewModel.cs
    ProviderKind.cs
    SyncStatus.cs
  Workspace/
    WorkspaceViewModel.cs
  FolderTree/
    FolderNode.cs
    CheckState.cs

// ❌ type grouping
src/
  ViewModels/
    AccountViewModel.cs
    WorkspaceViewModel.cs
    FolderNode.cs
  Enums/
    ProviderKind.cs
```

This applies to all new code.

## Classes and Methods

- Define one class, record, interface etc. per file, and name the file after the class. The exception to this is when defining multiple related record types for a discriminated union - in this case, all records should be defined in the same file. Factory classes for records should be defined in the same file as the record they relate to.
- Follow SOLID principles for class and method design.
- Keep classes / methods focused on a SINGLE responsibility.
- Ensure good cohesion within classes and methods (related functionality grouped together).
- Ensure low coupling between classes and methods.
- Ensure methods do one thing and do it well.
- Keep methods short; ideally under 20 lines.
- Keep classes short; ideally under 300 lines.
- Use meaningful names for classes and methods that clearly convey their purpose.
- Put all method / constructor overloads together in the same order as their parameters.
- Single-line method / constructor signatures where possible. Split parameters across lines ONLY if line-length > 200 characters and spilt as close to 200 characters as possible. Use as few lines as possible when splitting parameters across lines.
- Use expression-bodied members for simple methods and properties.
- Keep method and constructor parameters to a minimum (ideally <5 parameters); prefer using parameter objects when multiple parameters are needed.
- Avoid long parameter lists; consider using the Builder pattern for complex object construction.
- Use dependency injection for managing dependencies (no newing up services inside classes).
- Prefer composition over inheritance.
- Avoid deep nesting; use early returns and guard clauses.
- Do not use regions or #pragma to hide code; refactor instead.
- Never comment within methods or private members; if a comment is needed, it likely indicates the method is doing too much and should be refactored into smaller, more focused methods. Instead of comments, strive for self-explanatory code through clear naming and small method sizes.
- Every `return` statement after a code block must be preceded by a blank line. `return` after an `if` must NOT be followed by a blank line or `{ return; }`.
- Name for **meaning**: `customerId` not `id`, `isExpired` not `flag`.
- Use builders for test setup / test data creation.
- Prefer a nullable accumulator parameter with `?? []` over an overload pair where one simply calls the other with an empty collection:
  ```csharp
  // ❌ overload pair — the first adds nothing
  private static Task<Result<List<T>, E>> GetPagesAsync(Client c, Page page, CancellationToken ct)
      => GetPagesAsync(c, page, [], ct);
  private static Task<Result<List<T>, E>> GetPagesAsync(Client c, Page page, IReadOnlyCollection<T> acc, CancellationToken ct) { ... }

  // ✅ single method with nullable accumulator
  private static Task<Result<List<T>, E>> GetPagesAsync(Client c, Page page, IReadOnlyCollection<T>? acc, CancellationToken ct)
  {
      var items = (acc ?? []).Concat(GetItemsFromPage(page)).ToList();
      // ...
  }
  ```
- A private method whose entire body is a 1–2 combinator chain that forwards to another private method earns nothing — inline it at the call site:
  ```csharp
  // ❌ wrapper adds a layer for no reason
  private static Task<Result<List<DriveFolder>, GraphError>> GetDriveFoldersAsync(Client c, DriveFound d, RootFound r, CancellationToken ct)
      => GetFirstPageAsync(c, d, r, ct).BindAsync(page => GetAllPagesAsync(c, d, r, page, ct));

  // ✅ inline at the call site
  private static Task<Result<List<DriveFolder>, GraphError>> GetRootFoldersForDriveAsync(Client c, DriveFound d, CancellationToken ct)
      => GetRootAsync(c, d, ct)
          .BindAsync(root => GetFirstPageAsync(c, d, root, ct)
              .BindAsync(page => GetAllPagesAsync(c, d, root, page, null, ct)));
  ```

## Primitive Obsession

- Don't use string / GUID etc for domain concepts - create a specific type:
    - Id should be strongly-typed - use AStar.Dev.Source.Generators / AStar.Dev.Source.Generators.Attributes to standardise the generation. The Id value CAN be a string / GUID but the property MUST be strongly typed.
    - File info / Directory info should NOT be represented as a string. Either use the Testably abstraction or create a specific type (i.e. when only 3-5 properties are required)

## Immutability

- Prefer immutable data structures and objects where possible.
- Prefer `record` types for immutable data models and DTOs.
- Use `class` for entities with behavior or mutable state.
- Use `init` properties when immutability is desired.
- Use immutable collections: `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyDictionary<TKey, TValue>`.
- Use `with` expressions to create modified copies of immutable objects.

## Record Design

- Define record properties on the same line with the record declaration when possible.
- Accompany each record `<name>` with a corresponding `<name>Factory` static factory class.
- Expose static `Create` methods on the factory class for constructing instances of the record.
- Place argument validation logic within the factory methods.
- Never use the public constructor of a record directly; always use the factory methods.
- Use immutable collections (e.g., `IReadOnlyList<T>`, `IReadOnlyDictionary<TKey, TValue>`) for record properties that hold multiple values.
- Avoid methods on records; use extension methods instead for any behavior related to the record.

## Discriminated Unions

- Use records with inheritance to model discriminated unions.
- Define an abstract base record for the union type and derive specific case records from it.
- Place all case records in the same file as the base record.
- Define one static factory class per discriminated union type.
- Expose static `Create` methods on the factory class for constructing instances of each case record.

## Variables and Constants

- Use `var` when the type is obvious from the right-hand side; otherwise use explicit types for clarity.
- Use `const` for compile-time constants.
- Use `static readonly` for runtime constants.
- NO magic strings / numbers etc; use constants or enums instead.

## Collections and Data Structures

- Use `IEnumerable<T>` for collections that do not require indexing.
- Use `IReadOnlyList<T>` or `IReadOnlyCollection<T>` when immutability is desired.
- Use `StringBuilder` for concatenating multiple strings in loops or when performance is critical; otherwise use string interpolation for readability.
- Use Collection Initializers and Object Initializers for cleaner code when creating collections and objects.

## Control Flow and Logic

- Use pattern matching and switch expressions for clearer and more concise code when dealing with multiple conditions.
- Null checking with null-coalescing operators (`??`, `??=`, `?.`).
- Use `ArgumentNullException` (or `ArgumentNullException.ThrowIfNull`) in public constructors and methods.

## Test Classes

- Name test classes with the `Given` prefix to describe the context under test (e.g., `GivenAnAccount`, `GivenANullString`, `GivenADatabaseReadyForSync`). This matches the `c-sharp-senior-qa-specialist` convention.
- Name test methods using the `when_[action]_then_[outcome]` snake_case convention (e.g., `when_deleted_then_all_linked_rows_are_removed`). This matches the `c-sharp-senior-qa-specialist` convention.
- Use the Arrange-Act-Assert (AAA) pattern within test methods to structure the code clearly. Divide the method into three distinct sections: setup (Arrange), execution (Act), and verification (Assert). Do not comment these sections; the structure should be clear from the code itself. Separate these sections with a single blank line for readability.
- Use test data builders to create complex test objects, enhancing readability and maintainability of tests.
- Avoid logic in test methods; keep tests simple and focused on behavior verification.
- **Always** use Shouldly for every assertion — `Assert.*` is **banned** in new test code.
  ```csharp
  // ✅
  sut.Accounts.Count.ShouldBe(4);
  raisedProperties.ShouldContain(nameof(WorkspaceViewModel.SelectedAccount));

  // ❌
  Assert.Equal(4, sut.Accounts.Count);
  Assert.Contains(nameof(WorkspaceViewModel.SelectedAccount), raisedProperties);
  ```
- **If no test project exists** for the production project being worked on, create it **before** writing any production code. Set up correct NuGet references (xUnit v3, Shouldly) and a `<ProjectReference>` to the production project before writing the first test.
- Prefer real instances or hand-rolled test doubles. Use NSubstitute only when constructing a real dependency requires significant setup that obscures the test's intent (e.g. `IAuthService`, `IGraphService`, `ILogger<T>`). See `@.claude/rules/c-sharp-testing.md` for full rules including integration test patterns.
- Never add XML documentation or comments to test classes or test methods.

## Functional Extensions (AStar.Dev.Functional.Extensions)

See `@.claude/rules/functional-usage.md` for the full authoritative rules. Hard constraints repeated here for visibility:

- **Never** await a `Task<Result<T,E>>` into an intermediate variable just to call `.Match()` on the next line. Always chain `.MatchAsync()` directly on the task:
  ```csharp
  // ❌ wrong
  var result = await service.GetAsync(ct);
  var value = result.Match<string?>(ok => ok.Value, _ => null);

  // ✅ correct
  var value = await service.GetAsync(ct)
      .MatchAsync<TSuccess, TError, string?>(ok => ok.Value, _ => null);
  ```
- Error-branch code (logging, setting properties, returning sentinel values) belongs **inside** the error lambda of `Match`/`MatchAsync`, not in a separate `if` block after the call.
- Never use `is Result<T,E>.Ok` / `is not Result<T,E>.Ok` / `is Option<T>.Some` / `is Option<T>.None` pattern matching in production code — use `Match` or `MatchAsync` exclusively.
- **Never use `string` as `TError`** — every layer boundary defines a typed error discriminated union (e.g., `AuthError`, `GraphError`, `PersistenceError`, `SyncError`).
- `try/catch` is only permitted at infrastructure boundaries (Graph, MSAL, file system, EF Core) and the `async void` Timer callback in `SyncScheduler`. Service and domain layers never `catch`.
- Observable properties in ViewModels are plain C# types — never `Result<T,E>` or `Option<T>`. Set them inside `Match`/`MatchAsync` lambdas.

## XML Documentation

All public methods, properties, and types must carry complete XML documentation:

- `<summary>` required on every public member.
- `<param name="...">` documented for **every** parameter.
- `<returns>` documented for every non-`void` method.
- `<exception cref="...">` documented for every exception the method explicitly `throw`s.
- Classes/interfaces: `<summary>` required. Use `<inheritdoc />` on concrete implementations of an interface — not on the interface itself.

```csharp
/// <summary>Computes the percentage of storage used.</summary>
/// <param name="usedBytes">Bytes currently consumed.</param>
/// <param name="totalBytes">Total capacity in bytes.</param>
/// <returns>A value between 0.0 and 1.0 representing the fraction used.</returns>
/// <exception cref="DivideByZeroException">Thrown when <paramref name="totalBytes"/> is zero.</exception>
public static double ComputeUsedFraction(long usedBytes, long totalBytes) { … }
```

## Utilities

- The AStar.Dev.Utilities project has a plethora of helpers. Use them when suitable.
    - e.g.: rather than `Path.Combine("Directory1", "Directory2", "Directory3")` use: `"Directory1".CombinePaths("Directory2", "Directory3")` - it will not silently drop parameters, unlike Path.Combine.
