using AStar.Dev.CloudSyncFunctional.Auth;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Auth;

public sealed class GivenAnAccountProfile
{
    [Fact]
    public void when_create_is_called_then_properties_are_set()
    {
        var profile = AccountProfileFactory.Create("Test User", "test@example.com");

        profile.DisplayName.ShouldBe("Test User");
        profile.Email.ShouldBe("test@example.com");
    }
}
