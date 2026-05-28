namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Auth;

public sealed class GivenADriveId
{
    [Fact]
    public void when_create_is_called_then_value_is_set()
    {
        var id = new CloudSyncFunctional.Auth.DriveId("id");

        id.Value.ShouldBe("id");
    }
}
