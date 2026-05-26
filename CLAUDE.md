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
Core library. Two discriminated unions:

**`Result<TResult, TError>`** — abstract record with `Ok` and `Fail` subtypes plus extension methods:
- `Map` — transform success value, propagate failure
- `Bind` — chain operations that return `Result`
- `Tap` — side-effect on success/failure, return result unchanged
- `Match` — fold both cases to a single output type

Implicit conversions exist from `Result<TResult,TError>` to `TResult` and `TError` (returns `default!` for the wrong case).

**`Option<TResult, TError>`** — extends `Result<TResult, TError>`. Abstract record with `Some<TResult, TError>` and `None<TResult, TError>` subtypes. Same four extension methods (`Map`, `Bind`, `Tap`, `Match`) via `OptionExtensions`. Implicit conversions to `TResult` (from `Some`) and `TError` (from `None`). Semantically: presence (`Some`) vs absence (`None`) rather than success/failure.

### `src/AStar.Dev.OneDriveFunctional`
Avalonia 12 desktop app (WinExe). Uses ReactiveUI with compiled bindings. Entry point is `Program.cs`; MVVM wired via `MainWindowViewModel : ReactiveObject`. `AvaloniaUI.DiagnosticsSupport` is Debug-only.

## Testing

### `test/AStar.Dev.FunctionsParadigm.Tests.Unit`
xUnit v3 tests against the FunctionalParadigm library. `TreatWarningsAsErrors` is on. Test classes are named `GivenA<Type>` with methods named `when_<condition>_then_<expectation>`.

### C#/.NET Conventions

- Eliminate "what" comments by extracting well-named methods — NOT by moving them into XML docs.
- Blank line before every `return` (except `return` directly after `if`/`else`).

## Conventions

- Nullable and ImplicitUsings enabled across all projects.
- Test naming: `GivenA<Subject>` class, `when_..._then_...` method names (snake_case).
- No mocking framework — tests construct types directly.
- **Commit messages**: Conventional Commits — `feat(packages/core): ...`, `fix(apps/web/Portal.Blazor): ...`
- **Branch names**: `feature/...`, `bug/...`, `doc/...`; `main` ALWAYS deployable
- **Test projects**: Named `*.Tests.Unit` or `*.Tests.Integration` — auto-set `IsPackable=false`
- **Method signatures**: Always single-line regardless of param count — `public void Foo(string a, int b, CancellationToken cancellationToken = default)`. Never split params across lines. Every file type.
- **Comments**: Never restate what code says — any file type (`.cs`, `.csproj`, `.axaml`, config, etc.). Refactor to extract when needed. Only comment when _reason_ behind decision isn't derivable from code.
- **XML Comments**: all public methods/properties — see full spec in `.claude/rules/c-sharp-code-style.md` § XML Documentation.
    - Every `<param>`, `<returns>`, and `<exception>` must be documented where applicable.
    - Classes implementing interface: use `<inheritdoc />`, not class-level docs.


## Before Starting ANY Task

Three steps **MANDATORY** before single line of code. No exceptions, including spikes.

1. **Branch first** — run `git branch`, confirm not on `main`. If on main, create branch:

    ```bash
    git checkout -b feature/short-description<-issue-number>
    ```

    Naming: `feature/...`, `bug/...`, `doc/...`. NEVER commit to `main`. See @docs/git-instructions.md.

2. **Tests MANDATORY** — EVERY coding task MUST follow TDD. COMMIT FAILING TEST BEFORE writing production code.

3. **Scope** — implement ONLY what was asked. Stop and wait for review before touching any other phase or area. Honor all explicit style requirements (primary constructors, idiomatic `Match`/`MatchAsync`, no tuple-intermediate patterns).

## Branching & Commits

ALL development work MUST follow the GIT rules in: @docs/git-instructions.md

## Code Exploration

- Call Serena `initial_instructions` BEFORE exploring the codebase — no exceptions.
- Use `mcp__serena__find_symbol` and `mcp__serena__find_referencing_symbols` for symbol lookups — do NOT read whole files for exploration.
- Find ALL call sites and test files before touching production code.
- This project has a graphify knowledge graph at graphify-out/. See ##graphify section below for additional information

## Definition of Done

Before any coding task complete — commits and PRs included:

1. `dotnet build` affected projects — zero errors, zero warnings
2. `dotnet test` affected test projects — all pass except new TDD `RED` tests. COMMIT failing tests.
3. Write MINIMAL production code to pass test(s)
4. Request human review BEFORE committing.
5. Human requests changes? Implement, re-request review.
6. ONLY after human approval: commit to branch, raise GitHub PR.

## Verification Before Declaring Done

NEVER say "fixed", "done", or "complete" without explicit evidence:

- Run `dotnet build` — zero errors required. Paste exact output.
- Run `dotnet test` — paste the EXACT pass/fail count from raw terminal output. Do NOT summarise or self-report. New failures must be zero; pre-existing failures must be identified.
- Confirm ALL call sites and test files were found and updated before reporting completion.
- Trace the original bug/requirement through the code path and state in plain text WHY the change addresses it at the root cause.
- For sync/download bugs specifically: confirm the full flow (Graph API → persistence → sync logic) before touching any code. Write a failing reproducing test first; declare done only when it turns green.

Say "I believe this is fixed because…" — never just "fixed".

## Subagent Usage

- Use `c-sharp-qa` subagent for adding or expanding tests in C# files.
- Use `c-sharp-dev` subagent for implementing C# features.
- Use `c-sharp-reviewer` subagent for code review.
- When a subagent drifts off task or produces wrong output, take over directly — do not re-prompt the same agent repeatedly.

### Verifying Subagent Output

After ANY subagent completes, verify before trusting its report:

1. **Files**: `Read` every file the subagent claims to have written or modified — do NOT assume it succeeded.
2. **Tests**: Re-run `dotnet test` yourself and paste actual output. Never accept a subagent's "all tests pass" summary as truth.
3. **Diff**: Confirm the actual changes match what was requested.

If verification fails, take over directly — do not re-prompt the same subagent.

## graphify

This project has a graphify knowledge graph at graphify-out/.

Rules:
- Before answering architecture or codebase questions, read graphify-out/GRAPH_REPORT.md for god nodes and community structure
- If graphify-out/wiki/index.md exists, navigate it instead of reading raw files
- For cross-module "how does X relate to Y" questions, prefer `graphify query "<question>"`, `graphify path "<A>" "<B>"`, or `graphify explain "<concept>"` over grep — these traverse the graph's EXTRACTED + INFERRED edges instead of scanning files
- After modifying code files in this session, run `graphify update .` to keep the graph current (AST-only, no API cost)
