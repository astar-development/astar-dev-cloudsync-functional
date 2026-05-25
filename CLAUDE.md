# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run tests for a specific project (uses Microsoft.Testing.Platform — OutputType=Exe)
dotnet run --project test/AStar.Dev.FunctionsParadigm.Tests.Unit

# Run app
dotnet run --project src/AStar.Dev.OneDriveFunctional

# Run single test by name filter
dotnet run --project test/AStar.Dev.FunctionsParadigm.Tests.Unit -- --filter "when_an_ok_result"
```

## Architecture

Three projects in `AStar.Dev.OneDrive.Functional.slnx`, targeting **net10.0**:

### `src/AStar.Dev.FunctionalParadigm`
Core library. Implements a discriminated union `Result<TResult, TError>` (abstract record with `Ok` and `Fail` subtypes) plus extension methods:
- `Map` — transform success value, propagate failure
- `Bind` — chain operations that return `Result`
- `Tap` — side-effect on success/failure, return result unchanged
- `Match` — fold both cases to a single output type

Implicit conversions exist from `Result<TResult,TError>` to `TResult` and `TError` (returns `default!` for the wrong case).

### `src/AStar.Dev.OneDriveFunctional`
Avalonia 12 desktop app (WinExe). Uses ReactiveUI with compiled bindings. Entry point is `Program.cs`; MVVM wired via `MainWindowViewModel : ReactiveObject`. `AvaloniaUI.DiagnosticsSupport` is Debug-only.

### `test/AStar.Dev.FunctionsParadigm.Tests.Unit`
xUnit v3 tests against the FunctionalParadigm library. `TreatWarningsAsErrors` is on. Test classes are named `GivenA<Type>` with methods named `when_<condition>_then_<expectation>`.

## Conventions

- Nullable and ImplicitUsings enabled across all projects.
- Test naming: `GivenA<Subject>` class, `when_..._then_...` method names (snake_case).
- No mocking framework — tests construct types directly.
