# Log Viewer

The app includes an in-app log viewer. It uses Serilog as the logging backend, bridged to `ILogger<T>` via `Serilog.Extensions.Logging`. All production code uses `ILogger<T>` — never reference Serilog types outside the log viewer and startup configuration.

## Packages required

```xml
<PackageReference Include="Serilog" />
<PackageReference Include="Serilog.Extensions.Hosting" />   <!-- UseSerilog() -->
<PackageReference Include="Serilog.Extensions.Logging" />   <!-- ILogger<T> bridge -->
<PackageReference Include="Serilog.Sinks.File" />           <!-- rolling file sink -->
<PackageReference Include="System.Reactive" />              <!-- IObservable<LogEntry> -->
```

## Architecture

```
Serilog pipeline
  └─ InMemoryLogSink   (implements ILogEventSink + ILogEntryProvider)
  └─ File sink         (rolling, JSON, 7-day retention)

ILogEntryProvider      (consumed by LogViewerViewModel)
  ├─ IObservable<LogEntry> EntryAdded   — reactive stream for live tail
  └─ IReadOnlyList<LogEntry> GetSnapshot()  — initial load
```

`InMemoryLogSink` is registered as a Singleton. It is both the Serilog sink and the `ILogEntryProvider` implementation — register the same instance for both.

## InMemoryLogSink

- Ring buffer: **500 entries** maximum. When full, oldest entry is evicted.
- Thread-safe: `ConcurrentQueue<LogEntry>` for storage; `Subject<LogEntry>` (Rx) for the push stream.
- PII scrubbing applied before storage (see below).
- Implements `IDisposable` — dispose completes the `Subject`.

```csharp
public sealed class InMemoryLogSink : ILogEventSink, ILogEntryProvider, IDisposable
{
    public const int DefaultCapacity = 500;

    public IObservable<LogEntry> EntryAdded { get; }        // hot observable
    public IReadOnlyList<LogEntry> GetSnapshot();           // point-in-time copy
    public void Emit(LogEvent logEvent);                    // called by Serilog
    public void Dispose();
}
```

## LogEntry

```csharp
public sealed record LogEntry(DateTimeOffset Timestamp, LogLevel Level, string Message, string? Exception);
```

`LogLevel` uses `Microsoft.Extensions.Logging.LogLevel` — not Serilog's enum — so the ViewModel has no Serilog dependency.

## ILogEntryProvider contract

```csharp
public interface ILogEntryProvider
{
    IObservable<LogEntry> EntryAdded { get; }
    IReadOnlyList<LogEntry> GetSnapshot();
}
```

## PII scrubbing

Before a log message is stored, scrub:

- **Email addresses** — replace with `[email]`
- **Usernames** (the local OS username, obtained via `Environment.UserName`) — replace with `[username]`

Scrubbing is applied to the rendered message string. Use `Regex` with compiled options for performance. Scrub both the message and the exception string (if present).

```csharp
// Example patterns — adjust as needed
private static readonly Regex EmailRegex =
    new(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);

private static string Scrub(string input)
{
    input = EmailRegex.Replace(input, "[email]");
    if (!string.IsNullOrEmpty(_username))
        input = input.Replace(_username, "[username]", StringComparison.OrdinalIgnoreCase);
    return input;
}
```

Cache `Environment.UserName` at construction time — do not call it per log entry.

## Startup configuration

Configure Serilog in `Program.cs` before `BuildAvaloniaApp()`. Register the `InMemoryLogSink` instance first so it can be added to the Serilog pipeline:

```csharp
var inMemorySink = new InMemoryLogSink();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Sink(inMemorySink)
    .WriteTo.File(
        formatter: new JsonFormatter(),
        path: logFilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();
```

Then register `inMemorySink` in the DI container as both `ILogEventSink` and `ILogEntryProvider`:

```csharp
services.AddSingleton<InMemoryLogSink>(inMemorySink);
services.AddSingleton<ILogEntryProvider>(sp => sp.GetRequiredService<InMemoryLogSink>());
```

## LogViewerViewModel

Subscribes to `ILogEntryProvider.EntryAdded` and loads `GetSnapshot()` on construction. Marshals updates to the UI thread via `RxApp.MainThreadScheduler`. Exposes an `ObservableCollection<LogEntry>` bound to the view.

```csharp
public sealed class LogViewerViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public ObservableCollection<LogEntry> Entries { get; } = [];

    public LogViewerViewModel(ILogEntryProvider provider)
    {
        foreach (var entry in provider.GetSnapshot())
            Entries.Add(entry);

        provider.EntryAdded
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(entry => Entries.Add(entry))
            .DisposeWith(_disposables);
    }

    public void Dispose() => _disposables.Dispose();
}
```
