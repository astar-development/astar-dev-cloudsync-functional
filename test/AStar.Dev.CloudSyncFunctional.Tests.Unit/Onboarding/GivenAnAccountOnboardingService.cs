using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.CloudSyncFunctional.Domain;
using AStar.Dev.CloudSyncFunctional.Onboarding;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Onboarding;

public class GivenAnAccountOnboardingService
{
    private static AccountOnboardingService CreateSut() =>
        new(Substitute.For<ILogger<AccountOnboardingService>>());

    private static OneDriveAccount CreateAccount() =>
        new()
        {
            AccountId = "test-account-id",
            Profile = new AccountProfile("Test User", "test@example.com"),
            SelectedFolderIds = ["folder-1", "folder-2"]
        };

    [Fact]
    public async Task when_complete_onboarding_is_called_then_result_is_ok()
    {
        var sut = CreateSut();
        var account = CreateAccount();

        var result = await sut.CompleteOnboardingAsync(account, CancellationToken.None);

        result.ShouldBeOfType<Ok<OneDriveAccount, PersistenceError>>();
    }

    [Fact]
    public async Task when_complete_onboarding_is_called_then_returned_account_has_same_id()
    {
        var sut = CreateSut();
        var account = CreateAccount();

        var result = await sut.CompleteOnboardingAsync(account, CancellationToken.None);

        var ok = (Ok<OneDriveAccount, PersistenceError>)result;
        ok.Value.AccountId.ShouldBe("test-account-id");
    }

    [Fact]
    public async Task when_complete_onboarding_is_called_then_account_is_active()
    {
        var sut = CreateSut();
        var account = CreateAccount();

        var result = await sut.CompleteOnboardingAsync(account, CancellationToken.None);

        var ok = (Ok<OneDriveAccount, PersistenceError>)result;
        ok.Value.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task when_complete_onboarding_is_called_then_profile_is_preserved()
    {
        var sut = CreateSut();
        var account = CreateAccount();

        var result = await sut.CompleteOnboardingAsync(account, CancellationToken.None);

        var ok = (Ok<OneDriveAccount, PersistenceError>)result;
        ok.Value.Profile.Email.ShouldBe("test@example.com");
    }
}
