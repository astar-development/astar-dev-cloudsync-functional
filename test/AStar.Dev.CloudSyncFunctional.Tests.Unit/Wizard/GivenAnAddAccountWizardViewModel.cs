using AStar.Dev.CloudSyncFunctional.Accounts;
using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Graph;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.CloudSyncFunctional.Tests.Unit.Infrastructure;
using AStar.Dev.CloudSyncFunctional.Wizard;
using AStar.Dev.FunctionalParadigm;
using PersistenceDriveId = AStar.Dev.CloudSyncFunctional.Persistence.ValueObjects.DriveId;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Wizard;

public class GivenAnAddAccountWizardViewModel : IClassFixture<ReactiveUiFixture>
{
    private static AddAccountWizardViewModel CreateSut(IAuthService? auth = null, IGraphService? graph = null, IAccountOnboardingService? onboarding = null)
    {
        auth ??= Substitute.For<IAuthService>();
        graph ??= Substitute.For<IGraphService>();
        onboarding ??= Substitute.For<IAccountOnboardingService>();

        return new AddAccountWizardViewModel(auth, graph, onboarding);
    }

    [Fact]
    public void when_constructed_then_current_step_is_provider_selection()
    {
        var sut = CreateSut();

        sut.CurrentStep.ShouldBe(WizardStep.ProviderSelection);
    }

    [Fact]
    public void when_constructed_then_is_provider_selection_step_is_true()
    {
        var sut = CreateSut();

        sut.IsProviderSelectionStep.ShouldBeTrue();
    }

    [Fact]
    public void when_constructed_then_can_go_back_is_false()
    {
        var sut = CreateSut();

        sut.CanGoBack.ShouldBeFalse();
    }

    [Fact]
    public void when_constructed_then_is_signed_in_is_false()
    {
        var sut = CreateSut();

        sut.IsSignedIn.ShouldBeFalse();
    }

    [Fact]
    public void when_constructed_then_show_not_implemented_is_false()
    {
        var sut = CreateSut();

        sut.ShowNotImplemented.ShouldBeFalse();
    }

    [Fact]
    public void when_constructed_then_folders_is_empty()
    {
        var sut = CreateSut();

        sut.Folders.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_select_provider_one_drive_then_step_is_sign_in()
    {
        var sut = CreateSut();

        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        sut.CurrentStep.ShouldBe(WizardStep.SignIn);
    }

    [Fact]
    public async Task when_select_provider_one_drive_then_can_go_back_is_true()
    {
        var sut = CreateSut();

        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        sut.CanGoBack.ShouldBeTrue();
    }

    [Fact]
    public async Task when_select_provider_one_drive_then_show_not_implemented_is_false()
    {
        var sut = CreateSut();

        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        sut.ShowNotImplemented.ShouldBeFalse();
    }

    [Fact]
    public async Task when_select_provider_google_drive_then_step_stays_at_provider_selection()
    {
        var sut = CreateSut();

        await sut.SelectProvider.Execute(ProviderKind.GoogleDrive);

        sut.CurrentStep.ShouldBe(WizardStep.ProviderSelection);
    }

    [Fact]
    public async Task when_select_provider_google_drive_then_shows_not_implemented_message()
    {
        var sut = CreateSut();

        await sut.SelectProvider.Execute(ProviderKind.GoogleDrive);

        sut.ShowNotImplemented.ShouldBeTrue();
        sut.NotImplementedMessage.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task when_select_provider_dropbox_then_step_stays_at_provider_selection()
    {
        var sut = CreateSut();

        await sut.SelectProvider.Execute(ProviderKind.Dropbox);

        sut.CurrentStep.ShouldBe(WizardStep.ProviderSelection);
    }

    [Fact]
    public async Task when_select_provider_dropbox_then_shows_not_implemented_message()
    {
        var sut = CreateSut();

        await sut.SelectProvider.Execute(ProviderKind.Dropbox);

        sut.ShowNotImplemented.ShouldBeTrue();
        sut.NotImplementedMessage.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task when_select_provider_one_drive_then_not_implemented_is_hidden()
    {
        var sut = CreateSut();
        await sut.SelectProvider.Execute(ProviderKind.GoogleDrive);

        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        sut.ShowNotImplemented.ShouldBeFalse();
    }

    [Fact]
    public async Task when_select_provider_then_not_implemented_message_is_cleared_on_second_one_drive_select()
    {
        var sut = CreateSut();
        await sut.SelectProvider.Execute(ProviderKind.GoogleDrive);
        await sut.Back.Execute();

        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        sut.ShowNotImplemented.ShouldBeFalse();
    }

    [Fact]
    public async Task when_sign_in_succeeds_then_step_is_select_folders()
    {
        var auth = Substitute.For<IAuthService>();
        var profile = new AccountProfile("Test User", "test@example.com");
        var authResult = AuthResultFactory.Create("token", "test-account-id", profile, DateTimeOffset.UtcNow.AddHours(1));
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Ok<AuthResult, AuthError>(authResult)));

        var graph = Substitute.For<IGraphService>();
        graph.GetRootFoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>([new DriveFolder("id1", "Documents", null)])));

        var sut = CreateSut(auth: auth, graph: graph);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.SignIn.Execute();

        sut.CurrentStep.ShouldBe(WizardStep.SelectFolders);
    }

    [Fact]
    public async Task when_sign_in_succeeds_then_is_signed_in_is_true()
    {
        var auth = Substitute.For<IAuthService>();
        var profile = new AccountProfile("Test User", "test@example.com");
        var authResult = AuthResultFactory.Create("token", "test-account-id", profile, DateTimeOffset.UtcNow.AddHours(1));
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Ok<AuthResult, AuthError>(authResult)));

        var graph = Substitute.For<IGraphService>();
        graph.GetRootFoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>([])));

        var sut = CreateSut(auth: auth, graph: graph);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.SignIn.Execute();

        sut.IsSignedIn.ShouldBeTrue();
    }

    [Fact]
    public async Task when_sign_in_succeeds_then_waiting_for_auth_is_false()
    {
        var auth = Substitute.For<IAuthService>();
        var profile = new AccountProfile("Test", "test@example.com");
        var authResult = AuthResultFactory.Create("token", "test-account-id", profile, DateTimeOffset.UtcNow.AddHours(1));
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Ok<AuthResult, AuthError>(authResult)));

        var graph = Substitute.For<IGraphService>();
        graph.GetRootFoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>([])));

        var sut = CreateSut(auth: auth, graph: graph);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.SignIn.Execute();

        sut.IsWaitingForAuth.ShouldBeFalse();
    }

    [Fact]
    public async Task when_sign_in_succeeds_then_status_text_shows_email()
    {
        var auth = Substitute.For<IAuthService>();
        var profile = new AccountProfile("Test User", "test@example.com");
        var authResult = AuthResultFactory.Create("token", "test-account-id", profile, DateTimeOffset.UtcNow.AddHours(1));
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Ok<AuthResult, AuthError>(authResult)));

        var graph = Substitute.For<IGraphService>();
        graph.GetRootFoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>([])));

        var sut = CreateSut(auth: auth, graph: graph);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.SignIn.Execute();

        sut.SignInStatusText.ShouldContain("test@example.com");
    }

    [Fact]
    public async Task when_sign_in_cancelled_then_step_is_provider_selection()
    {
        var auth = Substitute.For<IAuthService>();
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Fail<AuthResult, AuthError>(AuthErrorFactory.Cancelled())));

        var sut = CreateSut(auth: auth);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.SignIn.Execute();

        sut.CurrentStep.ShouldBe(WizardStep.ProviderSelection);
    }

    [Fact]
    public async Task when_sign_in_cancelled_then_sign_in_has_no_error()
    {
        var auth = Substitute.For<IAuthService>();
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Fail<AuthResult, AuthError>(AuthErrorFactory.Cancelled())));

        var sut = CreateSut(auth: auth);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.SignIn.Execute();

        sut.SignInHasError.ShouldBeFalse();
    }

    [Fact]
    public async Task when_sign_in_fails_then_error_is_shown()
    {
        var auth = Substitute.For<IAuthService>();
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Fail<AuthResult, AuthError>(AuthErrorFactory.Failed("MSAL error"))));

        var sut = CreateSut(auth: auth);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.SignIn.Execute();

        sut.SignInHasError.ShouldBeTrue();
        sut.SignInStatusText.ShouldContain("MSAL error");
    }

    [Fact]
    public async Task when_sign_in_fails_then_step_stays_at_sign_in()
    {
        var auth = Substitute.For<IAuthService>();
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Fail<AuthResult, AuthError>(AuthErrorFactory.Failed("error"))));

        var sut = CreateSut(auth: auth);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.SignIn.Execute();

        sut.CurrentStep.ShouldBe(WizardStep.SignIn);
    }

    [Fact]
    public async Task when_back_on_sign_in_step_then_step_is_provider_selection()
    {
        var sut = CreateSut();
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.Back.Execute();

        sut.CurrentStep.ShouldBe(WizardStep.ProviderSelection);
    }

    [Fact]
    public async Task when_back_on_sign_in_step_then_can_go_back_is_false()
    {
        var sut = CreateSut();
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);

        await sut.Back.Execute();

        sut.CanGoBack.ShouldBeFalse();
    }

    [Fact]
    public async Task when_back_on_sign_in_step_then_sign_in_error_is_cleared()
    {
        var auth = Substitute.For<IAuthService>();
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Fail<AuthResult, AuthError>(AuthErrorFactory.Failed("error"))));

        var sut = CreateSut(auth: auth);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);
        await sut.SignIn.Execute();

        await sut.Back.Execute();

        sut.SignInHasError.ShouldBeFalse();
    }

    [Fact]
    public async Task when_back_on_select_folders_step_then_step_is_sign_in()
    {
        var auth = Substitute.For<IAuthService>();
        var profile = new AccountProfile("Test", "test@example.com");
        var authResult = AuthResultFactory.Create("token", "test-account-id", profile, DateTimeOffset.UtcNow.AddHours(1));
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Ok<AuthResult, AuthError>(authResult)));

        var graph = Substitute.For<IGraphService>();
        graph.GetRootFoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>([])));

        var sut = CreateSut(auth: auth, graph: graph);
        await sut.SelectProvider.Execute(ProviderKind.OneDrive);
        await sut.SignIn.Execute();

        await sut.Back.Execute();

        sut.CurrentStep.ShouldBe(WizardStep.SignIn);
    }

    [Fact]
    public async Task when_cancel_is_executed_then_cancelled_event_is_raised()
    {
        var sut = CreateSut();
        var eventRaised = false;
        sut.Cancelled += (_, _) => eventRaised = true;

        await sut.Cancel.Execute();

        eventRaised.ShouldBeTrue();
    }

    [Fact]
    public async Task when_add_account_succeeds_then_completed_event_is_raised()
    {
        var auth = Substitute.For<IAuthService>();
        var profile = new AccountProfile("Test User", "test@example.com");
        var authResult = AuthResultFactory.Create("token", "test-account-id", profile, DateTimeOffset.UtcNow.AddHours(1));
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Ok<AuthResult, AuthError>(authResult)));

        var graph = Substitute.For<IGraphService>();
        graph.GetRootFoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>([])));
        graph.GetDriveIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<PersistenceDriveId, GraphError>>(new Ok<PersistenceDriveId, GraphError>(new PersistenceDriveId("drive-id-1"))));

        var onboarding = Substitute.For<IAccountOnboardingService>();
        onboarding.CompleteOnboardingAsync(Arg.Any<OneDriveAccount>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult<Result<OneDriveAccount, PersistenceError>>(
                new Ok<OneDriveAccount, PersistenceError>(call.Arg<OneDriveAccount>())));

        var sut = CreateSut(auth: auth, graph: graph, onboarding: onboarding);
        OneDriveAccount? completedAccount = null;
        sut.Completed += (_, account) => completedAccount = account;

        await sut.SelectProvider.Execute(ProviderKind.OneDrive);
        await sut.SignIn.Execute();
        await sut.AddAccount.Execute();

        completedAccount.ShouldNotBeNull();
        completedAccount.Profile.Email.ShouldBe("test@example.com");
    }

    [Fact]
    public async Task when_add_account_fails_then_error_is_surfaced()
    {
        var auth = Substitute.For<IAuthService>();
        var profile = new AccountProfile("Test User", "test@example.com");
        var authResult = AuthResultFactory.Create("token", "test-account-id", profile, DateTimeOffset.UtcNow.AddHours(1));
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Ok<AuthResult, AuthError>(authResult)));

        var graph = Substitute.For<IGraphService>();
        graph.GetRootFoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>([])));
        graph.GetDriveIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<PersistenceDriveId, GraphError>>(new Ok<PersistenceDriveId, GraphError>(new PersistenceDriveId("drive-id-1"))));

        var onboarding = Substitute.For<IAccountOnboardingService>();
        onboarding.CompleteOnboardingAsync(Arg.Any<OneDriveAccount>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<OneDriveAccount, PersistenceError>>(
                new Fail<OneDriveAccount, PersistenceError>(new PersistenceUnexpectedError("DB failure"))));

        var sut = CreateSut(auth: auth, graph: graph, onboarding: onboarding);

        await sut.SelectProvider.Execute(ProviderKind.OneDrive);
        await sut.SignIn.Execute();
        await sut.AddAccount.Execute();

        sut.HasError.ShouldBeTrue();
        sut.ErrorMessage.ShouldContain("DB failure");
    }

    [Fact]
    public async Task when_get_drive_id_fails_then_error_is_surfaced()
    {
        var auth = Substitute.For<IAuthService>();
        var profile = new AccountProfile("Test User", "test@example.com");
        var authResult = AuthResultFactory.Create("token", "test-account-id", profile, DateTimeOffset.UtcNow.AddHours(1));
        auth.SignInInteractiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<AuthResult, AuthError>>(new Ok<AuthResult, AuthError>(authResult)));

        var graph = Substitute.For<IGraphService>();
        graph.GetRootFoldersAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<List<DriveFolder>, GraphError>>(new Ok<List<DriveFolder>, GraphError>([])));
        graph.GetDriveIdAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<PersistenceDriveId, GraphError>>(new Fail<PersistenceDriveId, GraphError>(GraphErrorFactory.Unexpected("drive lookup failed"))));

        var sut = CreateSut(auth: auth, graph: graph);

        await sut.SelectProvider.Execute(ProviderKind.OneDrive);
        await sut.SignIn.Execute();
        await sut.AddAccount.Execute();

        sut.HasError.ShouldBeTrue();
        sut.ErrorMessage.ShouldContain("drive lookup failed");
    }
}
