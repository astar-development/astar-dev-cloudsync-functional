namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Auth;

public sealed class GivenAnAccountId
{
    [Fact]
    public void when_create_is_called_then_value_is_set()
    {
        var id = new CloudSyncFunctional.Auth.AccountId("id");

        id.Value.ShouldBe("id");
    }
}
