using AStar.Dev.CloudSyncFunctional.Auth;
using AStar.Dev.FunctionalParadigm;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Auth;

public sealed class GivenAnAuthService
{
    private static AuthService CreateSut(IPublicClientApplication? app = null) =>
        new(app ?? Substitute.For<IPublicClientApplication>(), Substitute.For<ILogger<AuthService>>());

    private static IAccount CreateAccount(string identifier)
    {
        var account = Substitute.For<IAccount>();
        var homeAccountId = new Microsoft.Identity.Client.AccountId(identifier, identifier, "tenant");
        account.HomeAccountId.Returns(homeAccountId);
        return account;
    }

    [Fact]
    public async Task when_sign_in_throws_authentication_canceled_error_code_then_returns_cancelled_error()
    {
        var app = Substitute.For<IPublicClientApplication>();
        app.When(a => a.AcquireTokenInteractive(Arg.Any<IEnumerable<string>>()))
           .Do(_ => throw new MsalClientException("authentication_canceled", "Cancelled"));
        var sut = CreateSut(app);

        var result = await sut.SignInInteractiveAsync(TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<AuthResult, AuthError>>();
        fail.Error.ShouldBeOfType<AuthCancelledError>();
    }

    [Fact]
    public async Task when_sign_in_throws_user_canceled_error_code_then_returns_cancelled_error()
    {
        var app = Substitute.For<IPublicClientApplication>();
        app.When(a => a.AcquireTokenInteractive(Arg.Any<IEnumerable<string>>()))
           .Do(_ => throw new MsalClientException("user_canceled", "Cancelled"));
        var sut = CreateSut(app);

        var result = await sut.SignInInteractiveAsync(TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<AuthResult, AuthError>>();
        fail.Error.ShouldBeOfType<AuthCancelledError>();
    }

    [Fact]
    public async Task when_sign_in_throws_operation_cancelled_then_returns_cancelled_error()
    {
        var app = Substitute.For<IPublicClientApplication>();
        app.When(a => a.AcquireTokenInteractive(Arg.Any<IEnumerable<string>>()))
           .Do(_ => throw new OperationCanceledException());
        var sut = CreateSut(app);

        var result = await sut.SignInInteractiveAsync(TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<AuthResult, AuthError>>();
        fail.Error.ShouldBeOfType<AuthCancelledError>();
    }

    [Fact]
    public async Task when_sign_in_throws_msal_exception_then_returns_failed_error()
    {
        var app = Substitute.For<IPublicClientApplication>();
        app.When(a => a.AcquireTokenInteractive(Arg.Any<IEnumerable<string>>()))
           .Do(_ => throw new MsalServiceException("some_code", "MSAL went wrong"));
        var sut = CreateSut(app);

        var result = await sut.SignInInteractiveAsync(TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<AuthResult, AuthError>>();
        fail.Error.ShouldBeOfType<AuthFailedError>();
        fail.Error.Message.ShouldContain("MSAL went wrong");
    }

    [Fact]
    public async Task when_sign_in_throws_exception_then_returns_failed_error()
    {
        var app = Substitute.For<IPublicClientApplication>();
        app.When(a => a.AcquireTokenInteractive(Arg.Any<IEnumerable<string>>()))
           .Do(_ => throw new Exception("unexpected"));
        var sut = CreateSut(app);

        var result = await sut.SignInInteractiveAsync(TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<AuthResult, AuthError>>();
        fail.Error.ShouldBeOfType<AuthFailedError>();
        fail.Error.Message.ShouldContain("unexpected");
    }

    [Fact]
    public async Task when_no_cached_accounts_then_returns_account_not_found_error()
    {
        var app = Substitute.For<IPublicClientApplication>();
        app.GetAccountsAsync().Returns(Task.FromResult<IEnumerable<IAccount>>([]));
        var sut = CreateSut(app);

        var result = await sut.AcquireTokenSilentAsync("any-id", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<AuthResult, AuthError>>();
        fail.Error.Message.ShouldContain("Account not found");
    }

    [Fact]
    public async Task when_matching_account_found_but_ui_required_then_returns_failed_error()
    {
        var app = Substitute.For<IPublicClientApplication>();
        var account = CreateAccount("target-id");
        app.GetAccountsAsync().Returns(Task.FromResult<IEnumerable<IAccount>>([account]));
        app.When(a => a.AcquireTokenSilent(Arg.Any<IEnumerable<string>>(), Arg.Any<IAccount>()))
           .Do(_ => throw new MsalUiRequiredException("code", "message"));
        var sut = CreateSut(app);

        var result = await sut.AcquireTokenSilentAsync("target-id", TestContext.Current.CancellationToken);

        var fail = result.ShouldBeOfType<Fail<AuthResult, AuthError>>();
        fail.Error.Message.ShouldContain("Re-authentication required");
    }

    [Fact]
    public async Task when_sign_out_with_no_cached_accounts_then_remove_is_not_called()
    {
        var app = Substitute.For<IPublicClientApplication>();
        app.GetAccountsAsync().Returns(Task.FromResult<IEnumerable<IAccount>>([]));
        var sut = CreateSut(app);

        await sut.SignOutAsync("any-id", TestContext.Current.CancellationToken);

        await app.DidNotReceive().RemoveAsync(Arg.Any<IAccount>());
    }

    [Fact]
    public async Task when_sign_out_with_matching_account_then_remove_is_called()
    {
        var app = Substitute.For<IPublicClientApplication>();
        var account = CreateAccount("target-id");
        app.GetAccountsAsync().Returns(Task.FromResult<IEnumerable<IAccount>>([account]));
        var sut = CreateSut(app);

        await sut.SignOutAsync("target-id", TestContext.Current.CancellationToken);

        await app.Received(1).RemoveAsync(Arg.Any<IAccount>());
    }

    [Fact]
    public async Task when_two_accounts_cached_then_returns_both_identifiers()
    {
        var app = Substitute.For<IPublicClientApplication>();
        var account1 = CreateAccount("id-1");
        var account2 = CreateAccount("id-2");
        app.GetAccountsAsync().Returns(Task.FromResult<IEnumerable<IAccount>>([account1, account2]));
        var sut = CreateSut(app);

        var result = await sut.GetCachedAccountIdsAsync();

        result.ShouldContain("id-1");
        result.ShouldContain("id-2");
    }
}
