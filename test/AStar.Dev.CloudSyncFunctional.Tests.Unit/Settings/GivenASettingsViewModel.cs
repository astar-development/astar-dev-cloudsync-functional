using AStar.Dev.CloudSyncFunctional.Settings;
using AStar.Dev.CloudSyncFunctional.Tests.Unit.Infrastructure;

namespace AStar.Dev.CloudSyncFunctional.Tests.Unit.Settings;

public class GivenASettingsViewModel : IClassFixture<ReactiveUiFixture>
{
    [Fact]
    public void when_constructed_then_close_command_is_not_null()
    {
        var sut = new SettingsViewModel();

        sut.Close.ShouldNotBeNull();
    }

    [Fact]
    public void when_close_is_executed_then_closed_event_is_raised()
    {
        var sut = new SettingsViewModel();
        var eventRaised = false;
        sut.Closed += (_, _) => eventRaised = true;

        sut.Close.Execute().Subscribe();

        eventRaised.ShouldBeTrue();
    }
}
