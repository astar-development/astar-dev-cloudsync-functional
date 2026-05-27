using AStar.Dev.CloudSyncFunctional.Auth;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Auth;

public class GivenAnAuthResult
{
    private static readonly AccountProfile TestProfile = new("Test User", "test@example.com");
    private static readonly DateTimeOffset TestExpiry = DateTimeOffset.UtcNow.AddHours(1);

    [Fact]
    public void when_create_is_called_then_access_token_is_set()
    {
        var result = AuthResultFactory.Create("token123", "account-id", TestProfile, TestExpiry);

        result.AccessToken.ShouldBe("token123");
    }

    [Fact]
    public void when_create_is_called_then_account_id_is_set()
    {
        var result = AuthResultFactory.Create("token", "account-id-abc", TestProfile, TestExpiry);

        result.AccountId.ShouldBe("account-id-abc");
    }

    [Fact]
    public void when_create_is_called_then_profile_is_set()
    {
        var result = AuthResultFactory.Create("token", "id", TestProfile, TestExpiry);

        result.Profile.ShouldBeSameAs(TestProfile);
    }

    [Fact]
    public void when_create_is_called_then_expires_on_is_set()
    {
        var result = AuthResultFactory.Create("token", "id", TestProfile, TestExpiry);

        result.ExpiresOn.ShouldBe(TestExpiry);
    }

    [Fact]
    public void when_create_is_called_with_empty_access_token_then_throws()
    {
        Should.Throw<ArgumentException>(() => AuthResultFactory.Create("", "id", TestProfile, TestExpiry));
    }

    [Fact]
    public void when_create_is_called_with_empty_account_id_then_throws()
    {
        Should.Throw<ArgumentException>(() => AuthResultFactory.Create("token", "", TestProfile, TestExpiry));
    }

    [Fact]
    public void when_create_is_called_with_null_profile_then_throws()
    {
        Should.Throw<ArgumentNullException>(() => AuthResultFactory.Create("token", "id", null!, TestExpiry));
    }
}
