# OneDrive ViewModel Patterns — ReactiveUI

All ViewModels use **ReactiveUI** (`ReactiveObject`). The delete project uses `CommunityToolkit.Mvvm` — do not copy those patterns.

**Functional type rules for ViewModels** (from `@.claude/rules/functional-usage.md`):
- Observable properties are **never** `Result<T,E>` or `Option<T>` — they are plain C# types set inside `Match`/`MatchAsync`.
- `is Ok` / `is Some` / `is None` pattern matching is **banned** — use `Match`/`MatchAsync` exclusively.
- No `try/catch` in ViewModels — services return `Result`/`Option`, which handles all failure paths.

**Error surfacing — mandatory:**
- Every ViewModel that can fail must expose `string ErrorMessage` and `bool HasError` reactive properties.
- The error branch of every `MatchAsync` MUST set these — silent swallowing is banned.
- The UI binds to `HasError` and `ErrorMessage` and displays the message to the user.

**Domain model isolation:**
- `*Entity` types (persistence models) are **never** used in ViewModels or views.
- ViewModels consume domain models (`OneDriveAccount`, `SyncConflict`, etc.) and primitive types only.
- Mapping from `*Entity` to domain model is performed in the service layer before the result reaches the ViewModel.

## Observable properties

```csharp
// ❌ CommunityToolkit
[ObservableProperty]
public partial WizardStep CurrentStep { get; set; } = WizardStep.SignIn;

// ✅ ReactiveUI (C# 14 field keyword)
public WizardStep CurrentStep
{
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
} = WizardStep.SignIn;
```

Always use the C# 14 `field` keyword — no explicit backing field needed.

## Notifying dependent properties when a source property changes

```csharp
// ❌ CommunityToolkit
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(IsSignInStep))]
[NotifyPropertyChangedFor(nameof(CanGoNext))]
public partial WizardStep CurrentStep { get; set; }

// ✅ ReactiveUI
// Option A — raise in setter (simplest when the dependent is a computed property)
public WizardStep CurrentStep
{
    get;
    set
    {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged(nameof(IsSignInStep));
        this.RaisePropertyChanged(nameof(IsSelectFoldersStep));
        this.RaisePropertyChanged(nameof(CanGoNext));
    }
} = WizardStep.SignIn;

// Option B — WhenAnyValue in constructor (better for complex derived state)
this.WhenAnyValue(x => x.CurrentStep)
    .Subscribe(_ =>
    {
        this.RaisePropertyChanged(nameof(IsSignInStep));
        this.RaisePropertyChanged(nameof(CanGoNext));
    });
```

Prefer Option A for 1–4 dependents. Prefer Option B when multiple source properties feed the same derived property.

## Commands — synchronous

```csharp
// ❌ CommunityToolkit
[RelayCommand]
private void Back() { ... }

// ✅ ReactiveUI
public ReactiveCommand<Unit, Unit> Back { get; }

// In constructor:
Back = ReactiveCommand.Create(() => { ... });
```

## Commands — asynchronous

```csharp
// ❌ CommunityToolkit
[RelayCommand]
private async Task NextAsync() { ... }

// ✅ ReactiveUI
public ReactiveCommand<Unit, Unit> Next { get; }

// In constructor:
Next = ReactiveCommand.CreateFromTask(async ct => { ... });
```

## Commands with `canExecute`

```csharp
// ❌ CommunityToolkit
[RelayCommand(CanExecute = nameof(CanGoNext))]
private async Task NextAsync() { ... }

// ✅ ReactiveUI — pass an IObservable<bool>
var canGoNext = this.WhenAnyValue(x => x.IsSignedIn, x => x.CurrentStep,
    (signedIn, step) => step switch
    {
        WizardStep.SignIn => signedIn,
        _                 => true
    });

Next = ReactiveCommand.CreateFromTask(async ct => { ... }, canGoNext);
```

The `canExecute` observable automatically disables the command in the UI when it emits `false`. No manual `NotifyCanExecuteChanged()` call needed.

## Notifying canExecute from property changes (no manual call)

```csharp
// ❌ CommunityToolkit
IsSignedIn = true;
NextCommand.NotifyCanExecuteChanged();   // manual nudge

// ✅ ReactiveUI — the WhenAnyValue observable handles it automatically
// Just set the property; the canExecute observable re-evaluates
IsSignedIn = true;
```

## Wizard ViewModel skeleton

```csharp
public sealed class AddAccountWizardViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private CancellationTokenSource? _authCts;

    public WizardStep CurrentStep
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            this.RaisePropertyChanged(nameof(IsSignInStep));
            this.RaisePropertyChanged(nameof(IsSelectFoldersStep));
            this.RaisePropertyChanged(nameof(IsConfirmStep));
            this.RaisePropertyChanged(nameof(CanGoBack));
        }
    } = WizardStep.SignIn;

    public bool IsSignInStep      => CurrentStep == WizardStep.SignIn;
    public bool IsSelectFoldersStep => CurrentStep == WizardStep.SelectFolders;
    public bool IsConfirmStep     => CurrentStep == WizardStep.Confirm;
    public bool CanGoBack         => CurrentStep != WizardStep.SignIn;

    public bool IsSignedIn
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsWaitingForAuth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string SignInStatusText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    public bool SignInHasError
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ObservableCollection<WizardFolderItem> Folders { get; } = [];

    public ReactiveCommand<Unit, Unit> OpenBrowser { get; }
    public ReactiveCommand<Unit, Unit> Back        { get; }
    public ReactiveCommand<Unit, Unit> Next        { get; }
    public ReactiveCommand<Unit, Unit> Cancel      { get; }

    public event EventHandler<OneDriveAccount>? Completed;
    public event EventHandler?                  Cancelled;

    public AddAccountWizardViewModel(IAuthService authService, IGraphService graphService)
    {
        var canGoNext = this.WhenAnyValue(x => x.IsSignedIn, x => x.CurrentStep,
            (signedIn, step) => step == WizardStep.SignIn ? signedIn : true);

        OpenBrowser = ReactiveCommand.CreateFromTask(ct => OpenBrowserAsync(authService, cancellationToken));
        Back        = ReactiveCommand.Create(ExecuteBack);
        Next        = ReactiveCommand.CreateFromTask(ct => ExecuteNextAsync(graphService, cancellationToken), canGoNext);
        Cancel      = ReactiveCommand.CreateFromTask(ExecuteCancelAsync);
    }

    public void Dispose()
    {
        _disposables.Dispose();
        _authCts?.Dispose();
    }
}
```

## Event subscriptions — dispose pattern

Subscribe to observables via `_disposables`:

```csharp
this.WhenAnyValue(x => x.CurrentStep)
    .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSignInStep)))
    .DisposeWith(_disposables);
```

Always dispose the `CompositeDisposable` in `Dispose()`.

## UI thread marshalling

When an async command updates observable properties from a non-UI thread (e.g. after a Graph API call), marshal back to the UI thread:

```csharp
// Inside an async ReactiveCommand handler
var folders = await graphService.GetRootFoldersAsync(...);
await folders.MatchAsync<List<DriveFolder>, GraphError, Unit>(
    f =>
    {
        RxSchedulers.MainThreadScheduler.Schedule(() =>
        {
            foreach (var folder in f)
                Folders.Add(new WizardFolderItem(folder.Id, folder.Name));
        });
        return Unit.Default;
    },
    error =>
    {
        RxSchedulers.MainThreadScheduler.Schedule(() =>
        {
            HasError = true;
            ErrorMessage = error.Message;
        });
        return Unit.Default;
    });
```

## WizardStep enum

```csharp
public enum WizardStep { SignIn, SelectFolders, Confirm }
```

Three steps. Wizard raises `Completed` (with the new `OneDriveAccount`) or `Cancelled` when finished. The parent ViewModel subscribes to these events and navigates away.

## Wizard hosting — content control swap

The wizard is displayed via content control swap within the main window — **never** as a separate dialog window. The host ViewModel (e.g. `MainWindowViewModel`) controls which content is active.

```csharp
// Host ViewModel
public ReactiveObject? CurrentContent
{
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
}

// When "Add Account" is triggered:
private void OpenAddAccountWizard()
{
    var wizard = _serviceProvider.GetRequiredService<AddAccountWizardViewModel>();
    wizard.Completed += (_, account) => { CurrentContent = null; /* load account */ };
    wizard.Cancelled += (_, _) => CurrentContent = null;
    CurrentContent = wizard;
}
```

```xml
<!-- MainWindow.axaml — ContentControl bound to CurrentContent -->
<ContentControl Content="{Binding CurrentContent}"
                HorizontalContentAlignment="Stretch"
                VerticalContentAlignment="Stretch">
    <ContentControl.DataTemplates>
        <DataTemplate DataType="vm:AddAccountWizardViewModel">
            <views:AddAccountWizardView />
        </DataTemplate>
    </ContentControl.DataTemplates>
</ContentControl>
```

When `CurrentContent` is `null`, the main content (accounts list, sync status) is shown. Setting it to a ViewModel instance swaps the content. The `ContentControl` must live in a `*`-sized Grid row per the Avalonia scrollable view rules.

## Domain model isolation

`*Entity` classes are EF Core persistence models. They are **never** exposed above the service layer:

```csharp
// ❌ ViewModel using a persistence entity directly
public AccountEntity SelectedAccount { get; set; }

// ✅ ViewModel using the domain model
public OneDriveAccount SelectedAccount
{
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
}
```

Mapping from `*Entity` → domain model occurs in the service layer (not in the ViewModel, not in the repository). The ViewModel only ever sees domain models and primitive types.
