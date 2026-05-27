using AStar.Dev.CloudSyncFunctional.Auth;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Auth;

public class GivenAnAuthError
{
    [Fact]
    public void when_cancelled_is_called_then_result_is_auth_cancelled_error()
    {
        var error = AuthErrorFactory.Cancelled();

        error.ShouldBeOfType<AuthCancelledError>();
    }

    [Fact]
    public void when_cancelled_is_called_then_message_is_not_empty()
    {
        var error = AuthErrorFactory.Cancelled();

        error.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_failed_is_called_with_message_then_result_is_auth_failed_error()
    {
        var error = AuthErrorFactory.Failed("MSAL error");

        error.ShouldBeOfType<AuthFailedError>();
    }

    [Fact]
    public void when_failed_is_called_with_message_then_message_is_preserved()
    {
        var error = AuthErrorFactory.Failed("MSAL error");

        error.Message.ShouldBe("MSAL error");
    }

    [Fact]
    public void when_failed_is_called_with_null_message_then_default_message_is_used()
    {
        var error = AuthErrorFactory.Failed(null);

        error.Message.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void when_failed_is_called_with_whitespace_message_then_default_message_is_used()
    {
        var error = AuthErrorFactory.Failed("   ");

        error.Message.ShouldNotBeNullOrEmpty();
        error.Message.ShouldNotBe("   ");
    }
}
