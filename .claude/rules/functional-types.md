# functional types

This repo contains functional-style `Result<T, TError>` and `Option<T>` flows.

> **See also**: `@.claude/rules/functional-usage.md` — the authoritative rules for which type to use in which layer, typed error unions, banned patterns (no `is Ok`, no `string` as TError, no `try/catch` outside infrastructure), and ViewModel usage.

## Option<T> vs Option<T, TError>

| Use | When |
|---|---|
| `Option<T>` | Value may or may not be present; absence needs no explanation (e.g. `GetById` returning `None` = "not found") |
| `Option<T, TError>` | Value may be absent AND absence has a typed reason callers may branch on |

Do not use `T?` (nullable) where `Option<T>` is semantically correct.

## Getting started

### `Result<T, TError>`

A simple end-to-end example is shown below.

```csharp
using AStar.Dev.FunctionalTypes;
using AStar.Dev.FunctionalTypes.Core;

var directoryService = new DirectoryService();
var path = args.Length > 0 ? args[0] : "/home/jbarden/Pictures/Screenshots/";

static Result<string[], string> EnsureFilesPresent(string[] files) =>
    files.Length == 0
        ? new Fail<string[], string>("No files were found in the directory.")
        : new Ok<string[], string>(files);

var output = directoryService
    .GetFilesInDirectory(path)
    .Map(files => files.Select(file => $"FILE: {file}").ToArray())
    .Bind(EnsureFilesPresent)
    .Match(
        files => string.Join(Environment.NewLine, files),
        error => $"Failed to get files: {error}");

Console.WriteLine(output);
```

This is the common flow to follow:

1. Call a function that returns `Result<T, TError>`.
2. Use `Map` to transform the success value.
3. Use `Bind` when the next operation can also fail.
4. Use `Tap` when you want to observe or print the current value.
5. Use `Match` when you want to turn the `Result` into a final value.

### `Option<T>`

Use `Option<T>` when a value may or may not be present, with no error message needed.

```csharp
using AStar.Dev.FunctionalTypes;

static Option<string> FindFirstLargeFile(string[] files) =>
    files.FirstOrDefault(f => new FileInfo(f).Length > 1_000_000) is { } file
        ? new Some<string>(file)
        : new None<string>();

var path = args.Length > 0 ? args[0] : "/home/jbarden/Pictures/Screenshots/";

Option<string[]> files = new Some<string[]>(
    Directory.GetFiles(path, "*", SearchOption.AllDirectories));

var output = files
    .Bind(FindFirstLargeFile)
    .Match(
        file => $"First large file: {file}",
        () => "No large files found.");

Console.WriteLine(output);
```

`Option<T>` has two cases:

- `Some<T>` — a value is present.
- `None<T>` — no value.

The common flow:

1. Start with a value or call a function that returns `Option<T>`.
2. Use `Map` to transform the value if present.
3. Use `Bind` when the next step can also return an `Option<T>`.
4. Use `Filter` to discard values that fail a predicate.
5. Use `Tap` to observe or log without changing the option.
6. Use `Match` or `GetOrElse` to extract a final value.

## Helpers

### `Result<T, TError>` helpers

### `Map`
Use `Map` when you want to transform the **success** value and keep the failure unchanged.

```csharp
var result = directoryService
    .GetFilesInDirectory(path)
    .Map(files => files.Select(file => $"FILE: {file}").ToArray());
```

When to use `Map`:
- You have a successful value and want to convert it into another value.
- You do not want to change the error path.
- You want to keep the pipeline lazy and composable.

### `Bind`
Use `Bind` when the next step returns another `Result` and you want to compose the two operations.

```csharp
static Result<string[], string> EnsureFilesPresent(string[] files) =>
    files.Length == 0
        ? new Fail<string[], string>("No files were found in the directory.")
        : new Ok<string[], string>(files);

var result = directoryService
    .GetFilesInDirectory(path)
    .Bind(EnsureFilesPresent);
```

When to use `Bind`:
- The next operation can fail.
- You want to chain one `Result`-producing function into another.
- You want to avoid manually unwrapping and rewrapping success/failure values.

### `Tap`
Use `Tap` for **side effects** such as logging or printing, while preserving the original `Result`.

```csharp
directoryService
    .GetFilesInDirectory(path)
    .Tap(
        files => files.ToList().ForEach(Console.WriteLine),
        error => Console.WriteLine($"Failed to get files: {error}"));
```

When to use `Tap`:
- You want to observe or log the current value.
- You do not want to change the `Result`.
- You want a fluent chain that still keeps the original outcome intact.

### `Match`
Use `Match` when the pipeline is finished and you want to convert the `Result` into a single final value.

```csharp
var output = directoryService
    .GetFilesInDirectory(path)
    .Map(files => files.Select(file => $"FILE: {file}").ToArray())
    .Bind(EnsureFilesPresent)
    .Match(
        files => string.Join(Environment.NewLine, files),
        error => $"Failed to get files: {error}");

Console.WriteLine(output);
```

When to use `Match`:
- You need a final value that is **not** a `Result`.
- You are at the boundary of the application, such as rendering, logging, or returning a response.
- You want an explicit, total handling of both success and failure without manual branching.

## Suggested flow

### `Result<T, TError>`

1. `Map` to transform success data
2. `Bind` to add another `Result`-returning validation or operation
3. `Tap` for side effects
4. `Match` to finish the pipeline

### `Option<T>`

1. `Filter` to discard values that do not satisfy a condition
2. `Map` to transform a present value
3. `Bind` to chain another `Option`-returning operation
4. `Tap` for side effects
5. `Match` or `GetOrElse` to finish the pipeline

Both patterns keep the happy path clear while preserving failure or absence handling all the way through.

## `Option<T>` helpers

### `Map`
Transform the value inside `Some<T>`, pass `None<T>` through unchanged.

```csharp
Option<string> option = new Some<string>("hello");
Option<int> length = option.Map(s => s.Length); // Some(5)
```

When to use `Map`:
- You have a present value and want to convert it into another value.
- You do not want to alter the absent path.

### `Bind`
Chain an operation that may itself return `None<T>`.

```csharp
static Option<string> FindFirstLargeFile(string[] files) =>
    files.FirstOrDefault(f => new FileInfo(f).Length > 1_000_000) is { } file
        ? new Some<string>(file)
        : new None<string>();

Option<string[]> files = new Some<string[]>(Directory.GetFiles(path));
Option<string> largeFile = files.Bind(FindFirstLargeFile);
```

When to use `Bind`:
- The next operation can itself return `None<T>`.
- You want to compose two `Option`-returning functions without manual unwrapping.

### `Filter`
Discard a present value when it does not satisfy a predicate, turning `Some<T>` into `None<T>`.

```csharp
Option<int> positive = new Some<int>(42).Filter(n => n > 0);  // Some(42)
Option<int> negative = new Some<int>(-1).Filter(n => n > 0);  // None
```

When to use `Filter`:
- You want to gate a present value on a condition.
- A failing predicate should be treated as absence, not as an error.

### `Tap`
Observe or log the current state without changing the `Option<T>`.

```csharp
option.Tap(
    value => Console.WriteLine($"Found: {value}"),
    ()    => Console.WriteLine("Nothing found."));
```

When to use `Tap`:
- You want side effects such as logging.
- You do not want to change the `Option`.

### `Match`
Collapse `Option<T>` into a single final value by handling both cases.

```csharp
var message = option.Match(
    value  => $"Found: {value}",
    ()     => "Nothing found.");
```

When to use `Match`:
- You need a concrete value, not an `Option<T>`.
- You are at the boundary of the application and want exhaustive handling of both cases.

### `GetOrElse`
Extract the value from `Some<T>`, or return a fallback for `None<T>`.

```csharp
var value = option.GetOrElse("default");
```

When to use `GetOrElse`:
- You have a sensible default and want to exit the `Option` pipeline in one line.
- The absence case does not need separate logic.
