using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.FunctionalParadigm;
using Avalonia.Threading;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace AStar.Dev.CloudSyncFunctional.Wizard;

/// <summary>ViewModel for the multi-step add-account wizard.</summary>
public sealed class AddAccountWizardViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly IAuthService _authService;
    private readonly IGraphService _graphService;
    private readonly IAccountOnboardingService _onboardingService;
    private CancellationTokenSource? _authCts;
    private AuthResult? _authResult;

    /// <summary>Gets or sets the current wizard step.</summary>
    public WizardStep CurrentStep
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            this.RaisePropertyChanged(nameof(IsProviderSelectionStep));
            this.RaisePropertyChanged(nameof(IsSignInStep));
            this.RaisePropertyChanged(nameof(IsSelectFoldersStep));
            this.RaisePropertyChanged(nameof(CanGoBack));
        }
    } = WizardStep.ProviderSelection;

    /// <summary>Gets whether the wizard is on the provider-selection step.</summary>
    public bool IsProviderSelectionStep => CurrentStep == WizardStep.ProviderSelection;

    /// <summary>Gets whether the wizard is on the sign-in step.</summary>
    public bool IsSignInStep => CurrentStep == WizardStep.SignIn;

    /// <summary>Gets whether the wizard is on the folder-selection step.</summary>
    public bool IsSelectFoldersStep => CurrentStep == WizardStep.SelectFolders;

    /// <summary>Gets whether the Back button should be enabled.</summary>
    public bool CanGoBack => CurrentStep != WizardStep.ProviderSelection;

    /// <summary>Gets or sets whether the user has successfully signed in.</summary>
    public bool IsSignedIn
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets whether the authentication flow is in progress.</summary>
    public bool IsWaitingForAuth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets the status text displayed on the sign-in step.</summary>
    public string SignInStatusText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets whether the sign-in step has an error to display.</summary>
    public bool SignInHasError
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets the "not implemented" message shown for Google Drive and Dropbox.</summary>
    public string NotImplementedMessage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets whether the "not implemented" message should be visible.</summary>
    public bool ShowNotImplemented
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets whether the folder list is currently loading.</summary>
    public bool IsLoadingFolders
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets whether the wizard has a general error to display.</summary>
    public bool HasError
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>Gets or sets the general wizard error message.</summary>
    public string ErrorMessage
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    /// <summary>Gets the available OneDrive folders for the user to select.</summary>
    public ObservableCollection<WizardFolderItem> Folders { get; } = [];

    /// <summary>Gets the command that selects a cloud provider and advances or shows a message.</summary>
    public ReactiveCommand<ProviderKind, RxUnit> SelectProvider { get; }

    /// <summary>Gets the command that starts the MSAL interactive sign-in flow.</summary>
    public ReactiveCommand<RxUnit, RxUnit> SignIn { get; }

    /// <summary>Gets the command that navigates back to the previous wizard step.</summary>
    public ReactiveCommand<RxUnit, RxUnit> Back { get; }

    /// <summary>Gets the command that finalises account onboarding and raises <see cref="Completed"/>.</summary>
    public ReactiveCommand<RxUnit, RxUnit> AddAccount { get; }

    /// <summary>Gets the command that cancels the wizard and raises <see cref="Cancelled"/>.</summary>
    public ReactiveCommand<RxUnit, RxUnit> Cancel { get; }

    /// <summary>Raised when account onboarding completes successfully.</summary>
    public event EventHandler<OneDriveAccount>? Completed;

    /// <summary>Raised when the user cancels the wizard.</summary>
    public event EventHandler? Cancelled;

    /// <summary>Initialises a new <see cref="AddAccountWizardViewModel"/>.</summary>
    /// <param name="authService">The authentication service for MSAL sign-in.</param>
    /// <param name="graphService">The Graph service for fetching OneDrive folders.</param>
    /// <param name="onboardingService">The service that persists the completed account.</param>
    public AddAccountWizardViewModel(IAuthService authService, IGraphService graphService, IAccountOnboardingService onboardingService)
    {
        _authService = authService;
        _graphService = graphService;
        _onboardingService = onboardingService;

        var canSignIn = this.WhenAnyValue(x => x.IsWaitingForAuth, waiting => !waiting);

        SelectProvider = ReactiveCommand.CreateFromTask<ProviderKind, RxUnit>((kind, ct) => ExecuteSelectProviderAsync(kind, ct));
        SignIn = ReactiveCommand.CreateFromTask(ct => ExecuteSignInAsync(ct), canSignIn);
        Back = ReactiveCommand.Create(ExecuteBack);
        AddAccount = ReactiveCommand.CreateFromTask(ct => ExecuteAddAccountAsync(ct));
        Cancel = ReactiveCommand.CreateFromTask(ExecuteCancelAsync);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposables.Dispose();
        _authCts?.Dispose();
    }

    /// <summary>Test helper that raises the <see cref="Completed"/> event directly.</summary>
    /// <param name="account">The account to pass with the event.</param>
    internal void SimulateCompleted(OneDriveAccount account) => Completed?.Invoke(this, account);

    private async Task<RxUnit> ExecuteSelectProviderAsync(ProviderKind kind, CancellationToken ct = default)
    {
        ShowNotImplemented = false;
        NotImplementedMessage = string.Empty;

        if (kind == ProviderKind.OneDrive)
        {
            CurrentStep = WizardStep.SignIn;
        }
        else
        {
            ShowNotImplemented = true;
            NotImplementedMessage = "Coming soon — not implemented yet";
        }

        return RxUnit.Default;
    }

    private async Task ExecuteSignInAsync(CancellationToken ct)
    {
        _authCts?.Dispose();
        _authCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_authCts.Token, timeoutCts.Token);

        IsWaitingForAuth = true;
        SignInHasError = false;
        SignInStatusText = "Opening browser…";

        await _authService.SignInInteractiveAsync(linkedCts.Token)
            .MatchAsync(
                ok =>
                {
                    _authResult = ok;
                    IsSignedIn = true;
                    IsWaitingForAuth = false;
                    SignInStatusText = $"Signed in as {ok.Profile.Email}";
                    CurrentStep = WizardStep.SelectFolders;
                    _ = LoadFoldersAsync(ok, CancellationToken.None);
                },
                error =>
                {
                    IsWaitingForAuth = false;
                    if (error is AuthCancelledError && timeoutCts.IsCancellationRequested)
                    {
                        SignInHasError = true;
                        SignInStatusText = "Sign-in timed out. Please try again.";
                    }
                    else if (error is AuthCancelledError)
                    {
                        CurrentStep = WizardStep.ProviderSelection;
                        SignInStatusText = string.Empty;
                    }
                    else
                    {
                        SignInHasError = true;
                        SignInStatusText = error.Message;
                    }
                });
    }

    private async Task LoadFoldersAsync(AuthResult authResult, CancellationToken ct)
    {
        IsLoadingFolders = true;
        await _graphService.GetRootFoldersAsync(authResult.AccountId, authResult.AccessToken, ct)
            .MatchAsync(
                folders =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        Folders.Clear();
                        foreach (var folder in folders)
                            Folders.Add(new WizardFolderItem { FolderId = folder.Id, Name = folder.Name });
                        IsLoadingFolders = false;
                    });
                },
                error =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        HasError = true;
                        ErrorMessage = error.Message;
                        IsLoadingFolders = false;
                    });
                });
    }

    private void ExecuteBack()
    {
        SignInHasError = false;
        SignInStatusText = string.Empty;
        ShowNotImplemented = false;
        CurrentStep = CurrentStep switch
        {
            WizardStep.SignIn => WizardStep.ProviderSelection,
            WizardStep.SelectFolders => WizardStep.SignIn,
            _ => WizardStep.ProviderSelection
        };
    }

    private async Task ExecuteAddAccountAsync(CancellationToken ct)
    {
        if (_authResult is null)
            return;

        var account = new OneDriveAccount
        {
            AccountId = _authResult.AccountId,
            Profile = _authResult.Profile,
            SelectedFolderIds = Folders.Where(f => f.IsSelected).Select(f => f.FolderId).ToList()
        };

        await _onboardingService.CompleteOnboardingAsync(account, ct)
            .MatchAsync(
                finalAccount => Completed?.Invoke(this, finalAccount),
                error =>
                {
                    HasError = true;
                    ErrorMessage = error.Message;
                });
    }

    private Task ExecuteCancelAsync(CancellationToken ct = default)
    {
        _authCts?.Cancel();
        Cancelled?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }
}
